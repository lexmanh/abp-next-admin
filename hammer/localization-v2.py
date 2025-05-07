import os
import json
from openai import OpenAI
from pathlib import Path
import time
import fnmatch # Để so khớp tên tệp theo mẫu
import re      # Để sử dụng biểu thức chính quy

# --- CẤU HÌNH ---
OPENAI_API_KEY = os.getenv("OPENAI_API_KEY")
OPENAI_MODEL = "gpt-4o"
TARGET_FILENAME = "vi.json"
TARGET_LANGUAGE = "Vietnamese"
API_CALL_DELAY_SECONDS = 2
# --- KẾT THÚC CẤU HÌNH ---

# (Các hàm translate_texts_object_openai và process_repo giữ nguyên như ở phiên bản trước)
def translate_texts_object_openai(texts_object_to_translate: dict, source_lang_description: str) -> dict | None:
    if not texts_object_to_translate:
        return {}
    if not OPENAI_API_KEY:
        print("Lỗi: Biến môi trường OPENAI_API_KEY chưa được đặt.")
        return None
    try:
        client = OpenAI(api_key=OPENAI_API_KEY)
    except Exception as e:
        print(f"Lỗi khi khởi tạo OpenAI client: {e}")
        return None

    json_content_str = json.dumps(texts_object_to_translate, ensure_ascii=False, indent=2)
    system_prompt = (
        "You are a highly skilled translation assistant. Your task is to translate the string values within a JSON object, "
        "which represents a collection of localization texts (key-value pairs). "
        "You must only translate the string values into the target language. Keep all keys identical. "
        "The output must be a valid JSON object strictly mirroring the input's key structure, containing only the translated texts. "
        "Do not add any explanatory text or markdown formatting around the JSON object itself."
    )
    user_prompt = (
        f"Translate the string values in the following JSON object from '{source_lang_description}' to {TARGET_LANGUAGE}. "
        "Keep all keys identical. The output must be a valid JSON object with the same keys as the input, "
        "and only the string values translated.\n\n"
        f"Input JSON (object containing texts to translate):\n```json\n{json_content_str}\n```\n\n"
        f"Output JSON ({TARGET_LANGUAGE}, translated texts object):"
    )
    print(f"  Đang gửi {len(texts_object_to_translate)} cặp key-value từ '{source_lang_description}' để dịch sang {TARGET_LANGUAGE}...")
    try:
        if API_CALL_DELAY_SECONDS > 0:
            # Kiểm tra xem đây có phải là lần gọi API đầu tiên của toàn bộ script không
            # Để đơn giản, sẽ thêm độ trễ trước mỗi lần gọi nếu có gì đó để dịch
            print(f"  Đợi {API_CALL_DELAY_SECONDS} giây trước khi gọi API...")
            time.sleep(API_CALL_DELAY_SECONDS)

        completion = client.chat.completions.create(
            model=OPENAI_MODEL,
            response_format={"type": "json_object"},
            messages=[
                {"role": "system", "content": system_prompt},
                {"role": "user", "content": user_prompt}
            ]
        )
        translated_texts_str = completion.choices[0].message.content
        if translated_texts_str:
            try:
                translated_texts_data = json.loads(translated_texts_str)
                final_translated_texts = {}
                missing_keys_from_model = []
                for key_original in texts_object_to_translate:
                    if key_original in translated_texts_data:
                        final_translated_texts[key_original] = translated_texts_data[key_original]
                    else:
                        missing_keys_from_model.append(key_original)
                        final_translated_texts[key_original] = texts_object_to_translate[key_original]
                if missing_keys_from_model:
                    print(f"  Cảnh báo: Model có thể đã bỏ sót {len(missing_keys_from_model)} key trong quá trình dịch: {', '.join(missing_keys_from_model)}. Các giá trị gốc đã được giữ lại.")
                print(f"  Đã nhận bản dịch cho {len(final_translated_texts)} cặp key-value từ OpenAI.")
                return final_translated_texts
            except json.JSONDecodeError as e:
                print(f"  Lỗi: OpenAI không trả về JSON object hợp lệ cho 'texts'. Lỗi: {e}")
                print(f"  Nội dung nhận được: {translated_texts_str}")
                return None
        else:
            print("  Lỗi: OpenAI trả về nội dung trống cho 'texts'.")
            return None
    except Exception as e:
        print(f"  Lỗi khi gọi API OpenAI: {e}")
        return None

def process_repo(repo_path_str: str):
    repo_path = Path(repo_path_str)
    if not repo_path.is_dir():
        print(f"Lỗi: Đường dẫn kho lưu trữ không hợp lệ: {repo_path_str}")
        return

    print(f"Đang quét kho lưu trữ để dịch tệp JSON: {repo_path.resolve()}")
    total_vi_files_created_or_updated = 0
    total_source_sets_processed = 0
    total_texts_actually_sent_to_api = 0

    for current_dir_str, _, files_in_current_dir in os.walk(repo_path):
        current_path = Path(current_dir_str)
        en_json_path = current_path / "en.json"
        zh_hans_json_path = current_path / "zh-Hans.json"
        target_vi_json_path = current_path / TARGET_FILENAME
        has_en = en_json_path.is_file()
        has_zh_hans = zh_hans_json_path.is_file()

        if not has_en and not has_zh_hans:
            continue

        total_source_sets_processed += 1
        print(f"\nĐang xử lý thư mục JSON: {current_path}")
        existing_vi_texts = {}
        existing_vi_full_data = None
        if target_vi_json_path.is_file():
            print(f"  Tìm thấy tệp đích đã tồn tại: {target_vi_json_path.name}")
            try:
                with open(target_vi_json_path, 'r', encoding='utf-8-sig') as f_vi:
                    existing_vi_full_data = json.load(f_vi)
                    if isinstance(existing_vi_full_data.get("texts"), dict):
                        existing_vi_texts = existing_vi_full_data["texts"]
                        print(f"  Đã tải {len(existing_vi_texts)} bản dịch hiện có từ {target_vi_json_path.name}.")
                    else:
                        print(f"  Cảnh báo: {target_vi_json_path.name} không có key 'texts' hợp lệ. Sẽ tạo mới 'texts'.")
                        existing_vi_full_data = None
            except Exception as e:
                print(f"  Lỗi khi đọc {target_vi_json_path.name}: {e}. Sẽ tạo mới nếu cần.")
                existing_vi_full_data = None
        merged_source_texts = {}
        en_culture_data_for_metadata = None
        zh_hans_culture_data_for_metadata = None
        zh_hans_texts_data_temp = {}
        if has_zh_hans:
            print(f"  Đọc tệp: {zh_hans_json_path.name}")
            try:
                with open(zh_hans_json_path, 'r', encoding='utf-8-sig') as f:
                    zh_hans_content = json.load(f)
                    zh_hans_culture_data_for_metadata = zh_hans_content
                    if isinstance(zh_hans_content.get("texts"), dict):
                        zh_hans_texts_data_temp = zh_hans_content["texts"]
            except Exception as e:
                print(f"  Lỗi khi đọc/parse {zh_hans_json_path.name}: {e}")
        en_texts_data_temp = {}
        if has_en:
            print(f"  Đọc tệp: {en_json_path.name}")
            try:
                with open(en_json_path, 'r', encoding='utf-8-sig') as f:
                    en_content = json.load(f)
                    en_culture_data_for_metadata = en_content
                    if isinstance(en_content.get("texts"), dict):
                        en_texts_data_temp = en_content["texts"]
            except Exception as e:
                print(f"  Lỗi khi đọc/parse {en_json_path.name}: {e}")
        merged_source_texts.update(zh_hans_texts_data_temp)
        merged_source_texts.update(en_texts_data_temp)
        if not merged_source_texts:
            print(f"  Không có văn bản nguồn ('texts') để xử lý tại {current_path}.")
            if existing_vi_full_data is not None:
                existing_vi_full_data["culture"] = "vi"
                if "texts" not in existing_vi_full_data : existing_vi_full_data["texts"] = {}
                try:
                    with open(target_vi_json_path, 'w', encoding='utf-8') as f_out:
                        json.dump(existing_vi_full_data, f_out, ensure_ascii=False, indent=2)
                    print(f"  Chỉ cập nhật culture cho {target_vi_json_path.name}.")
                    total_vi_files_created_or_updated +=1
                except Exception as e:
                    print(f"  Lỗi khi ghi lại {target_vi_json_path.name}: {e}")
            continue
        texts_to_send_for_translation = {}
        for key, source_value in merged_source_texts.items():
            if key not in existing_vi_texts:
                texts_to_send_for_translation[key] = source_value
        newly_translated_texts_object = {}
        if texts_to_send_for_translation:
            print(f"  Tìm thấy {len(texts_to_send_for_translation)} texts mới/chưa dịch cần gửi API.")
            total_texts_actually_sent_to_api += len(texts_to_send_for_translation)
            source_lang_description = ""
            if has_en and has_zh_hans:
                source_lang_description = "primarily English, with potential Simplified Chinese phrases"
            elif has_en:
                source_lang_description = "English"
            elif has_zh_hans:
                source_lang_description = "Simplified Chinese"
            temp_translated = translate_texts_object_openai(texts_to_send_for_translation, source_lang_description)
            if temp_translated:
                newly_translated_texts_object = temp_translated
            else:
                print(f"  Dịch thuật API không thành công cho các văn bản mới tại {current_path}.")
        else:
            print(f"  Không có văn bản mới nào cần dịch cho {current_path}.")
        final_combined_vi_texts = existing_vi_texts.copy()
        final_combined_vi_texts.update(newly_translated_texts_object)
        final_vi_json_structure = {}
        if existing_vi_full_data is not None:
            final_vi_json_structure = {k: v for k, v in existing_vi_full_data.items() if k != "texts"}
        elif en_culture_data_for_metadata:
            final_vi_json_structure = {k: v for k, v in en_culture_data_for_metadata.items() if k != "texts"}
        elif zh_hans_culture_data_for_metadata:
            final_vi_json_structure = {k: v for k, v in zh_hans_culture_data_for_metadata.items() if k != "texts"}
        final_vi_json_structure["culture"] = "vi"
        final_vi_json_structure["texts"] = final_combined_vi_texts
        try:
            with open(target_vi_json_path, 'w', encoding='utf-8') as f_out:
                json.dump(final_vi_json_structure, f_out, ensure_ascii=False, indent=2)
            print(f"  Đã cập nhật/lưu thành công tệp: {target_vi_json_path}")
            total_vi_files_created_or_updated += 1
        except Exception as e:
            print(f"  Lỗi khi ghi tệp {target_vi_json_path}: {e}")
    print(f"\n--- Hoàn tất xử lý JSON ---")
    print(f"Đã xử lý {total_source_sets_processed} bộ tệp nguồn (en.json và/hoặc zh-Hans.json).")
    print(f"Đã gửi tổng cộng {total_texts_actually_sent_to_api} cặp key-value mới/chưa dịch đến API.")
    print(f"Đã tạo/cập nhật thành công {total_vi_files_created_or_updated} tệp {TARGET_FILENAME}.")

def add_vietnamese_to_module_files(repo_path_str: str):
    """
    Quét kho lưu trữ để tìm các tệp Module C# và thêm thông tin ngôn ngữ Tiếng Việt
    vào khối options.Languages.Add(...) nếu chưa có.
    """
    repo_path = Path(repo_path_str)
    if not repo_path.is_dir():
        print(f"Lỗi: Đường dẫn kho lưu trữ không hợp lệ cho việc cập nhật tệp Module: {repo_path_str}")
        return

    print(f"\nĐang quét kho lưu trữ để cập nhật tệp Module C# cho ngôn ngữ Tiếng Việt...")

    # Thông tin dòng cần thêm cho Tiếng Việt
    vietnamese_lang_info_signature = 'new LanguageInfo("vi", "vi", "Tiếng Việt")'
    # Regex để kiểm tra sự tồn tại của dòng Tiếng Việt một cách linh hoạt hơn về khoảng trắng
    vietnamese_present_regex = re.compile(r'options\.Languages\.Add\(\s*new\s+LanguageInfo\(\s*"vi",\s*"vi",\s*"Tiếng Việt"\s*\)\s*\);')

    # Regex để tìm một dòng options.Languages.Add bất kỳ để xác định vị trí chèn và thụt lề
    # Nó sẽ bắt nhóm (group 1) là phần thụt lề đầu dòng
    add_line_regex = re.compile(r"^(\s*)options\.Languages\.Add\(new LanguageInfo\(.*\)\s*;\s*$")

    files_to_check_patterns = ["Module.*.cs", "*Module.cs"]
    module_files_updated = 0
    module_files_scanned = 0

    for current_dir_str, _, files_in_current_dir in os.walk(repo_path):
        for filename in files_in_current_dir:
            matches_pattern = any(fnmatch.fnmatch(filename.lower(), pattern.lower()) for pattern in files_to_check_patterns)
            if not matches_pattern:
                continue

            module_files_scanned += 1
            file_path = Path(current_dir_str) / filename
            print(f"  Kiểm tra tệp C#: {file_path}")

            try:
                with open(file_path, 'r', encoding='utf-8-sig') as f: # utf-8-sig để xử lý BOM nếu có
                    lines = f.readlines()

                # Kiểm tra xem Tiếng Việt đã được thêm chưa
                already_has_vietnamese = any(vietnamese_present_regex.search(line) for line in lines)

                if already_has_vietnamese:
                    print(f"    Ngôn ngữ Tiếng Việt đã tồn tại trong {filename}.")
                    continue

                # Nếu chưa có, tìm vị trí để chèn
                last_add_line_index = -1
                indentation = "            " # Thụt lề mặc định (12 dấu cách) nếu không tìm thấy dòng mẫu

                for i, line in reversed(list(enumerate(lines))):
                    match = add_line_regex.match(line)
                    if match:
                        last_add_line_index = i
                        indentation = match.group(1) # Lấy thụt lề từ dòng tìm thấy
                        break

                new_vietnamese_line_to_add = f'{indentation}options.Languages.Add(new LanguageInfo("vi", "vi", "Tiếng Việt"));\n'

                if last_add_line_index != -1:
                    # Chèn vào sau dòng options.Languages.Add(...) cuối cùng tìm thấy
                    lines.insert(last_add_line_index + 1, new_vietnamese_line_to_add)
                    print(f"    Đã thêm thông tin Tiếng Việt vào {filename}.")
                else:
                    # Nếu không tìm thấy bất kỳ dòng options.Languages.Add nào,
                    # cố gắng tìm khối Configure<AbpLocalizationOptions>
                    # Đây là một giải pháp dự phòng đơn giản, có thể cần điều chỉnh tùy cấu trúc file
                    config_block_indices = [i for i, line in enumerate(lines) if "Configure<AbpLocalizationOptions>" in line and "options =>" in line]

                    inserted_in_block = False
                    if config_block_indices:
                        # Lấy chỉ số của dòng Configure... cuối cùng (nếu có nhiều)
                        block_start_line_index = config_block_indices[-1]
                        # Cố gắng tìm dấu '{' sau dòng này
                        open_brace_index = -1
                        for i in range(block_start_line_index, len(lines)):
                            if "{" in lines[i]:
                                open_brace_index = i
                                # Lấy thụt lề của dòng có dấu { và thêm 4 dấu cách
                                base_indent = re.match(r"^(\s*)", lines[open_brace_index]).group(1)
                                block_indentation = base_indent + "    " # Thêm 4 dấu cách
                                new_vietnamese_line_to_add = f'{block_indentation}options.Languages.Add(new LanguageInfo("vi", "vi", "Tiếng Việt"));\n'
                                lines.insert(open_brace_index + 1, new_vietnamese_line_to_add)
                                print(f"    Đã thêm Tiếng Việt vào khối Configure<AbpLocalizationOptions> trong {filename}.")
                                inserted_in_block = True
                                break

                    if not inserted_in_block:
                        print(f"    CẢNH BÁO: Không tìm thấy dòng 'options.Languages.Add(...)' hoặc khối 'Configure<AbpLocalizationOptions>' phù hợp trong {filename}. Bỏ qua việc thêm tự động.")
                        continue # Bỏ qua sửa đổi tệp này

                # Ghi lại nội dung đã sửa đổi
                with open(file_path, 'w', encoding='utf-8') as f: # Giữ nguyên encoding utf-8 khi ghi
                    f.writelines(lines)
                module_files_updated += 1

            except Exception as e:
                print(f"  Lỗi khi xử lý tệp {file_path}: {e}")

    print(f"\n--- Hoàn tất cập nhật tệp Module C# ---")
    print(f"Đã quét {module_files_scanned} tệp Module phù hợp.")
    print(f"Đã cập nhật {module_files_updated} tệp với thông tin ngôn ngữ Tiếng Việt.")


if __name__ == "__main__":
    if not OPENAI_API_KEY:
        print("Vui lòng đặt biến môi trường OPENAI_API_KEY trước khi chạy script.")
    else:
        repo_directory_input = input("Nhập đường dẫn đến thư mục kho lưu trữ của bạn (để trống nếu là thư mục hiện tại): ").strip()
        if not repo_directory_input:
            repo_directory_input = "." # Mặc định là thư mục hiện tại nếu không nhập gì

        # Bước 1: Dịch các tệp JSON
        process_repo(repo_directory_input)

        # Bước 2: Cập nhật các tệp Module C#
        add_vietnamese_to_module_files(repo_directory_input)

        print("\nHoàn thành tất cả các tác vụ!")