import os
import json
from openai import OpenAI
from pathlib import Path
import time
import fnmatch
import re

# --- CẤU HÌNH ---
OPENAI_API_KEY = os.getenv("OPENAI_API_KEY")
OPENAI_MODEL = "gpt-4o"
TARGET_JSON_FILENAME = "vi.json" # Đổi tên để rõ ràng hơn
TARGET_LANGUAGE = "Vietnamese"
API_CALL_DELAY_SECONDS = 1 # Giảm độ trễ một chút

# --- TÙY CHỌN XỬ LÝ BỔ SUNG ---
DRY_RUN_DEFAULT = False
FORCE_RETRANSLATE_JSON_DEFAULT = False
# --- KẾT THÚC CẤU HÌNH ---

# (Hàm translate_texts_object_openai và process_repo giữ nguyên như bản trước)
def translate_texts_object_openai(texts_object_to_translate: dict, source_lang_description: str, dry_run: bool = False) -> dict | None:
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
    print(f"  Đang gửi {len(texts_object_to_translate)} cặp key-value JSON từ '{source_lang_description}' để dịch sang {TARGET_LANGUAGE}...")
    try:
        if API_CALL_DELAY_SECONDS > 0:
            print(f"  Đợi {API_CALL_DELAY_SECONDS} giây trước khi gọi API (JSON)...")
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
                    print(f"  Cảnh báo: Model có thể đã bỏ sót {len(missing_keys_from_model)} key JSON trong quá trình dịch: {', '.join(missing_keys_from_model)}. Các giá trị gốc đã được giữ lại.")
                print(f"  Đã nhận bản dịch cho {len(final_translated_texts)} cặp key-value JSON từ OpenAI.")
                return final_translated_texts
            except json.JSONDecodeError as e:
                print(f"  Lỗi: OpenAI không trả về JSON object hợp lệ cho 'texts'. Lỗi: {e}")
                print(f"  Nội dung nhận được: {translated_texts_str}")
                return None
        else:
            print("  Lỗi: OpenAI trả về nội dung trống cho 'texts'.")
            return None
    except Exception as e:
        print(f"  Lỗi khi gọi API OpenAI (dịch JSON): {e}")
        return None

def process_repo(repo_path_str: str, dry_run: bool = False, force_retranslate: bool = False):
    repo_path = Path(repo_path_str).resolve()
    if not repo_path.is_dir():
        print(f"Lỗi: Đường dẫn kho lưu trữ không hợp lệ: {repo_path_str}")
        return

    if dry_run: print("--- CHẾ ĐỘ DRY RUN ĐANG BẬT (JSON) ---")
    if force_retranslate: print("--- TÙY CHỌN BUỘC DỊCH LẠI ĐANG BẬT (JSON) ---")

    print(f"Đang quét kho lưu trữ để dịch tệp JSON: {repo_path}")
    # ... (phần còn lại của hàm process_repo giữ nguyên như bản trước) ...
    # Chỉ đảm bảo tên biến TARGET_FILENAME được dùng đúng
    # Ví dụ: target_vi_json_path = current_path / TARGET_JSON_FILENAME (nếu bạn đổi tên biến)
    total_vi_files_created_or_updated = 0
    total_source_sets_processed = 0
    total_texts_actually_sent_to_api = 0

    for current_dir_str, dirnames, files_in_current_dir in os.walk(repo_path):
        current_path = Path(current_dir_str)
        if current_path != repo_path:
            try:
                path_relative_to_repo = current_path.relative_to(repo_path)
                if any(part.startswith('.') for part in path_relative_to_repo.parts):
                    dirnames[:] = []
                    continue
            except ValueError:
                dirnames[:] = []
                continue
        en_json_path = current_path / "en.json"
        zh_hans_json_path = current_path / "zh-Hans.json"
        target_vi_json_path = current_path / TARGET_JSON_FILENAME
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
                if not dry_run:
                    try:
                        with open(target_vi_json_path, 'w', encoding='utf-8') as f_out:
                            json.dump(existing_vi_full_data, f_out, ensure_ascii=False, indent=2)
                        print(f"  Chỉ cập nhật culture cho {target_vi_json_path.name}.")
                        total_vi_files_created_or_updated +=1
                    except Exception as e:
                        print(f"  Lỗi khi ghi lại {target_vi_json_path.name}: {e}")
                else:
                    print(f"  DRY RUN: Sẽ chỉ cập nhật culture cho {target_vi_json_path.name} (không ghi tệp).")
                    total_vi_files_created_or_updated +=1
            continue
        texts_to_send_for_translation = {}
        if force_retranslate:
            print("  BUỘC DỊCH LẠI: Tất cả các key từ nguồn sẽ được đưa vào danh sách dịch.")
            texts_to_send_for_translation = merged_source_texts.copy()
        else:
            for key, source_value in merged_source_texts.items():
                if key not in existing_vi_texts:
                    texts_to_send_for_translation[key] = source_value
        newly_translated_texts_object = {}
        if texts_to_send_for_translation:
            num_keys_to_send = len(texts_to_send_for_translation)
            print(f"  {'Sẽ gửi (DRY RUN)' if dry_run else 'Tìm thấy'} {num_keys_to_send} texts cần dịch {'(hoặc dịch lại)' if force_retranslate and merged_source_texts else '(mới/chưa dịch)'}.")
            total_texts_actually_sent_to_api += num_keys_to_send
            source_lang_description = ""
            if has_en and has_zh_hans:
                source_lang_description = "primarily English, with potential Simplified Chinese phrases"
            elif has_en:
                source_lang_description = "English"
            elif has_zh_hans:
                source_lang_description = "Simplified Chinese"
            temp_translated = translate_texts_object_openai(texts_to_send_for_translation, source_lang_description, dry_run)
            if temp_translated is not None:
                newly_translated_texts_object = temp_translated
            else:
                if not dry_run:
                    print(f"  Dịch thuật API không thành công cho các văn bản mới tại {current_path}.")
        else:
            print(f"  Không có văn bản mới nào cần dịch cho {current_path} (hoặc không buộc dịch lại).")
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
        if not dry_run:
            try:
                with open(target_vi_json_path, 'w', encoding='utf-8') as f_out:
                    json.dump(final_vi_json_structure, f_out, ensure_ascii=False, indent=2)
                print(f"  Đã cập nhật/lưu thành công tệp: {target_vi_json_path}")
                total_vi_files_created_or_updated += 1
            except Exception as e:
                print(f"  Lỗi khi ghi tệp {target_vi_json_path}: {e}")
        else:
            print(f"  DRY RUN: Sẽ cập nhật/lưu tệp: {target_vi_json_path} với {len(final_combined_vi_texts)} texts.")
            sample_texts_dry_run = dict(list(final_combined_vi_texts.items())[:3]) if final_combined_vi_texts else {}
            if len(final_combined_vi_texts) > 3: sample_texts_dry_run["..."] = "..."
            print(f"  DRY RUN: Nội dung texts giả lập (một phần): {json.dumps(sample_texts_dry_run, ensure_ascii=False, indent=2)}")
            total_vi_files_created_or_updated += 1
    print(f"\n--- Hoàn tất xử lý JSON ---")
    print(f"Đã xử lý {total_source_sets_processed} bộ tệp nguồn (en.json và/hoặc zh-Hans.json).")
    print(f"Tổng số key-value {'sẽ được gửi (DRY RUN)' if dry_run else 'đã được gửi'} để dịch: {total_texts_actually_sent_to_api}.")
    print(f"Tổng số tệp {TARGET_JSON_FILENAME} {'sẽ được (DRY RUN)' if dry_run else 'đã được'} tạo/cập nhật: {total_vi_files_created_or_updated}.")

def add_vietnamese_to_module_files(repo_path_str: str, dry_run: bool = False):
    repo_path = Path(repo_path_str).resolve()
    if not repo_path.is_dir():
        print(f"Lỗi: Đường dẫn kho lưu trữ không hợp lệ cho việc cập nhật tệp Module: {repo_path_str}")
        return

    print(f"\nĐang quét kho lưu trữ để cập nhật tệp Module C# cho ngôn ngữ Tiếng Việt...")
    if dry_run: print("--- CHẾ ĐỘ DRY RUN ĐANG BẬT (C# Module) ---")

    vietnamese_present_regex = re.compile(r'options\.Languages\.Add\s*\(\s*new\s+LanguageInfo\s*\(\s*"vi",\s*"vi",\s*"Tiếng Việt"\s*\)\s*\)\s*;')
    english_present_regex = re.compile(r'options\.Languages\.Add\s*\(\s*new\s+LanguageInfo\s*\(\s*"en",')
    chinese_present_regex = re.compile(r'options\.Languages\.Add\s*\(\s*new\s+LanguageInfo\s*\(\s*"zh-Hans",')
    add_line_regex = re.compile(r"^(\s*)(options\.Languages\.Add\(new LanguageInfo\(.*\)\s*;)\s*$")

    files_to_check_patterns = ["*Module.*.cs", "*Module.cs"]
    module_files_updated = 0
    module_files_scanned = 0
    is_comment_regex = re.compile(r"^\s*//")

    for current_dir_str, dirnames, files_in_current_dir in os.walk(repo_path):
        current_path = Path(current_dir_str)
        if current_path != repo_path:
            try:
                path_relative_to_repo = current_path.relative_to(repo_path)
                if any(part.startswith('.') for part in path_relative_to_repo.parts):
                    dirnames[:] = []
                    continue
            except ValueError:
                dirnames[:] = []
                continue

        for filename in files_in_current_dir:
            matches_pattern = any(fnmatch.fnmatch(filename.lower(), pattern.lower()) for pattern in files_to_check_patterns)
            if not matches_pattern:
                continue

            module_files_scanned += 1
            file_path = current_path / filename
            print(f"  Kiểm tra tệp C# Module: {file_path}")
            try:
                with open(file_path, 'r', encoding='utf-8-sig') as f:
                    lines = f.readlines()
                original_lines_for_dry_run = list(lines)
                already_has_vietnamese = False
                for line_content in lines:
                    if not is_comment_regex.match(line_content) and vietnamese_present_regex.search(line_content):
                        already_has_vietnamese = True
                        break
                if already_has_vietnamese:
                    print(f"    Ngôn ngữ Tiếng Việt đã tồn tại trong {filename}.")
                    continue
                has_en_config = False
                for line_content in lines:
                    if not is_comment_regex.match(line_content) and english_present_regex.search(line_content):
                        has_en_config = True
                        break
                has_zh_config = False
                for line_content in lines:
                    if not is_comment_regex.match(line_content) and chinese_present_regex.search(line_content):
                        has_zh_config = True
                        break
                if not (has_en_config or has_zh_config):
                    print(f"    Không tìm thấy cấu hình cho 'en' hoặc 'zh-Hans' trong {filename}. Bỏ qua.")
                    continue
                last_add_line_index = -1
                indentation = "            "
                for i, line_content in reversed(list(enumerate(lines))):
                    if not is_comment_regex.match(line_content):
                        match_any_add = add_line_regex.match(line_content)
                        if match_any_add:
                            last_add_line_index = i
                            indentation = match_any_add.group(1)
                            break
                new_vietnamese_line_to_add = f'{indentation}options.Languages.Add(new LanguageInfo("vi", "vi", "Tiếng Việt"));\n'
                action_taken_on_cs_file = False
                if last_add_line_index != -1:
                    lines.insert(last_add_line_index + 1, new_vietnamese_line_to_add)
                    action_taken_on_cs_file = True
                else:
                    print(f"    CẢNH BÁO: Tìm thấy 'en'/'zh-Hans' nhưng không tìm được dòng 'options.Languages.Add' phù hợp để chèn vào {filename}. Bỏ qua.")
                    continue
                if action_taken_on_cs_file:
                    if not dry_run:
                        with open(file_path, 'w', encoding='utf-8') as f:
                            f.writelines(lines)
                        print(f"    Đã cập nhật {filename} với thông tin Tiếng Việt.")
                    else:
                        print(f"    DRY RUN: Sẽ cập nhật {filename} với thông tin Tiếng Việt.")
                    module_files_updated += 1
            except Exception as e:
                print(f"  Lỗi khi xử lý tệp C# {file_path}: {e}")
    print(f"\n--- Hoàn tất cập nhật tệp Module C# ---")
    print(f"Đã quét {module_files_scanned} tệp Module phù hợp với mẫu.")
    print(f"Tổng số tệp Module C# {'sẽ được (DRY RUN)' if dry_run else 'đã được'} cập nhật: {module_files_updated}.")

# --- HÀM MỚI ĐỂ DỊCH displayName TRONG NAVIGATION ---
def translate_single_text_openai(text: str, source_lang_name: str, target_lang: str, dry_run: bool = False) -> str | None:
    """
    Dịch một đoạn văn bản đơn lẻ.
    """
    if not text.strip(): # Bỏ qua nếu chuỗi rỗng hoặc chỉ chứa khoảng trắng
        return text
    if not OPENAI_API_KEY:
        # print("Lỗi: Biến môi trường OPENAI_API_KEY chưa được đặt. (translate_single_text)") # Có thể bỏ qua để tránh lặp lại
        return None # Hoặc trả về text gốc để không làm hỏng file

    if dry_run:
        return f"[DRY_RUN_{target_lang.upper()}_FOR_{text[:30].replace newline('')}...]"

    try:
        client = OpenAI(api_key=OPENAI_API_KEY)
    except Exception as e:
        print(f"Lỗi khi khởi tạo OpenAI client (single_text): {e}")
        return None

    system_prompt = f"You are a concise translation assistant. Your primary task is to translate the provided text snippet accurately from {source_lang_name} to {target_lang}. Return only the translated text itself, without any additional explanations, introductory phrases, or surrounding characters like quotes unless they are part of the translation itself."
    user_prompt = f"Translate the following {source_lang_name} text to {target_lang}:\n\nOriginal text: \"{text}\"\n\nTranslated text only:"

    print(f"    Đang dịch displayName: '{text}' từ {source_lang_name} sang {target_lang}...")
    try:
        if API_CALL_DELAY_SECONDS > 0:
            print(f"      Đợi {API_CALL_DELAY_SECONDS} giây trước khi gọi API (displayName)...")
            time.sleep(API_CALL_DELAY_SECONDS)

        completion = client.chat.completions.create(
            model=OPENAI_MODEL,
            messages=[
                {"role": "system", "content": system_prompt},
                {"role": "user", "content": user_prompt}
            ],
            temperature=0.3, # Giảm nhiệt độ để bản dịch sát nghĩa hơn
            max_tokens=len(text.split()) * 3 + 20 # Ước lượng token cho output
        )
        translated = completion.choices[0].message.content.strip()
        # Đôi khi model vẫn có thể trả về dấu ngoặc kép bao quanh, thử loại bỏ nếu có
        if translated.startswith('"') and translated.endswith('"') and len(translated) > 1:
            translated = translated[1:-1]
        return translated
    except Exception as e:
        print(f"    Lỗi khi dịch văn bản '{text}': {e}")
        return None # Trả về None nếu lỗi, hoặc text gốc để không làm hỏng file

def translate_display_names_in_navigation_providers(repo_path_str: str, dry_run: bool = False):
    repo_path = Path(repo_path_str).resolve()
    if not repo_path.is_dir():
        print(f"Lỗi: Đường dẫn kho lưu trữ không hợp lệ cho việc dịch displayName: {repo_path_str}")
        return

    print(f"\nĐang quét kho lưu trữ để dịch displayName trong *NavigationDefinitionProvider.cs...")
    if dry_run:
        print("--- CHẾ ĐỘ DRY RUN ĐANG BẬT (C# displayName) ---")

    # Regex để tìm `displayName:` theo sau là một chuỗi C# (có thể là thường hoặc verbatim @).
    # Group 1: Phần trước chuỗi (ví dụ `displayName: `)
    # Group 2: Toàn bộ chuỗi C# bao gồm dấu ngoặc kép và @ nếu có (ví dụ `"仪表盘"` hoặc `@ "仪表盘"`)
    # Group 3: Nội dung thực sự của chuỗi bên trong dấu ngoặc kép (ví dụ `仪表盘`)
    display_name_pattern = re.compile(
        r'(displayName\s*:\s*)'  # Group 1: "displayName: " (prefix)
        r'(@?"((?:\\.|[^"\\])*)")'  # Group 2: Toàn bộ chuỗi ký tự (ví dụ "text" hoặc @"text")
        # Group 3: Nội dung của chuỗi (bên trong group 2)
    )

    files_to_check_pattern = "*NavigationDefinitionProvider.cs" # Chỉ một mẫu
    files_changed_count = 0
    total_display_names_found = 0
    total_display_names_translated_successfully = 0 # Đếm số lần dịch thành công và có thay đổi

    for root_dir_str, dirnames, filenames_in_dir in os.walk(repo_path):
        current_path_obj = Path(root_dir_str)

        # Logic bỏ qua thư mục ẩn
        if current_path_obj != repo_path:
            try:
                path_relative_to_repo = current_path_obj.relative_to(repo_path)
                if any(part.startswith('.') for part in path_relative_to_repo.parts):
                    dirnames[:] = []
                    continue
            except ValueError:
                dirnames[:] = []
                continue

        for filename_str in filenames_in_dir:
            if not fnmatch.fnmatch(filename_str.lower(), files_to_check_pattern.lower()):
                continue

            file_path_obj = current_path_obj / filename_str
            print(f"  Đang xử lý tệp Navigation: {file_path_obj}")

            try:
                with open(file_path_obj, 'r', encoding='utf-8-sig') as f:
                    original_content = f.read()

                modified_content_parts = []
                last_match_end = 0
                file_modified_flag = False

                for match in display_name_pattern.finditer(original_content):
                    total_display_names_found += 1

                    prefix_capture = match.group(1) # Ví dụ: 'displayName: '
                    string_literal_capture = match.group(2) # Ví dụ: '"仪表盘"' hoặc '@"nội dung"'
                    chinese_text_to_translate = match.group(3) # Ví dụ: '仪表盘' hoặc 'nội dung'

                    # Thêm phần nội dung từ lần khớp cuối cùng đến đầu lần khớp này
                    modified_content_parts.append(original_content[last_match_end:match.start()])

                    print(f"    Tìm thấy displayName: '{chinese_text_to_translate}'")

                    # Giả định rằng các chuỗi này là tiếng Trung cần dịch
                    # (Có thể thêm logic kiểm tra ngôn ngữ ở đây nếu cần độ chính xác cao hơn)
                    translated_text = translate_single_text_openai(
                        chinese_text_to_translate,
                        "Simplified Chinese",
                        TARGET_LANGUAGE,
                        dry_run
                    )

                    if translated_text is not None and translated_text != chinese_text_to_translate:
                        # Xử lý escaping cho chuỗi C# mới
                        new_string_literal = ""
                        if string_literal_capture.startswith('@"'): # Chuỗi verbatim
                            new_string_literal = f'@"{translated_text.replace("\"", "\"\"")}"' # Escape " thành ""
                        else: # Chuỗi thường
                            escaped_translated_text = translated_text.replace('\\', '\\\\').replace('"', '\\"')
                            new_string_literal = f'"{escaped_translated_text}"'

                        modified_content_parts.append(prefix_capture)
                        modified_content_parts.append(new_string_literal)
                        print(f"      -> Dịch thành (literal): {new_string_literal}")
                        total_display_names_translated_successfully +=1
                        file_modified_flag = True
                    else:
                        # Giữ lại toàn bộ match gốc nếu không dịch, lỗi, hoặc dry run không có thay đổi giả lập
                        modified_content_parts.append(match.group(0))
                        if translated_text is None and not dry_run: # Lỗi dịch thực sự
                            print(f"      -> Lỗi dịch, giữ nguyên: '{chinese_text_to_translate}'")
                        elif dry_run and translated_text is not None : # Dry run có kết quả giả lập
                            print(f"      -> DRY RUN: Sẽ thay thế bằng: '{translated_text}'")
                            # total_display_names_translated_successfully +=1 # Đã đếm ở trên khi gọi hàm
                            file_modified_flag = True # Coi như sẽ có thay đổi
                        else: # Không có gì để dịch hoặc bản dịch giống hệt
                            print(f"      -> Không cần dịch hoặc không thay đổi, giữ nguyên: '{chinese_text_to_translate}'")

                    last_match_end = match.end()

                # Thêm phần còn lại của tệp (sau lần khớp cuối cùng)
                modified_content_parts.append(original_content[last_match_end:])

                if file_modified_flag:
                    if not dry_run:
                        with open(file_path_obj, 'w', encoding='utf-8') as f:
                            f.write("".join(modified_content_parts))
                        print(f"    Đã cập nhật displayName(s) trong {filename_str}")
                    else:
                        print(f"    DRY RUN: Sẽ cập nhật displayName(s) trong {filename_str}")
                    files_changed_count += 1
                elif total_display_names_found > 0 and any(m.group(3) for m in display_name_pattern.finditer(original_content)): # Nếu có displayName được tìm thấy nhưng không có gì thay đổi
                    print(f"    Không có thay đổi nào được thực hiện cho displayName(s) trong {filename_str}.")

            except Exception as e:
                print(f"  Lỗi khi xử lý tệp Navigation Provider {file_path_obj}: {e}")

    print(f"\n--- Hoàn tất dịch displayName trong C# Navigation Providers ---")
    print(f"Tổng số displayName tìm thấy: {total_display_names_found}")
    if not dry_run:
        print(f"Tổng số displayName đã dịch thành công và có thay đổi: {total_display_names_translated_successfully}")
    else:
        print(f"Tổng số displayName sẽ được dịch (DRY RUN): {total_display_names_translated_successfully}")
    print(f"Tổng số tệp *NavigationDefinitionProvider.cs {'sẽ được (DRY RUN)' if dry_run else 'đã được'} cập nhật: {files_changed_count}")


if __name__ == "__main__":
    if not OPENAI_API_KEY:
        print("Vui lòng đặt biến môi trường OPENAI_API_KEY trước khi chạy script.")
    else:
        repo_directory_input = input("Nhập đường dẫn đến thư mục kho lưu trữ của bạn (để trống nếu là thư mục hiện tại): ").strip()
        if not repo_directory_input:
            repo_directory_input = "."

        dry_run_choice = input("Chạy ở chế độ DRY RUN (chỉ xem trước, không thay đổi file)? (y/n, mặc định n): ").strip().lower()
        DRY_RUN_SCRIPT = True if dry_run_choice == 'y' else DRY_RUN_DEFAULT

        force_retranslate_json_choice = input("Buộc DỊCH LẠI TẤT CẢ các key JSON (thay vì chỉ dịch key mới)? (y/n, mặc định n): ").strip().lower()
        FORCE_RETRANSLATE_JSON_SCRIPT = True if force_retranslate_json_choice == 'y' else FORCE_RETRANSLATE_JSON_DEFAULT

        print("-" * 30)
        if DRY_RUN_SCRIPT:
            print("CHẾ ĐỘ DRY RUN: KÍCH HOẠT")
        if FORCE_RETRANSLATE_JSON_SCRIPT:
            print("BUỘC DỊCH LẠI JSON: KÍCH HOẠT")
        print("-" * 30)

        # Bước 1: Dịch các tệp JSON
        process_repo(repo_directory_input, dry_run=DRY_RUN_SCRIPT, force_retranslate=FORCE_RETRANSLATE_JSON_SCRIPT)

        # Bước 2: Cập nhật các tệp Module C# (thêm LanguageInfo)
        add_vietnamese_to_module_files(repo_directory_input, dry_run=DRY_RUN_SCRIPT)

        # Bước 3: Dịch displayName trong các tệp *NavigationDefinitionProvider.cs
        translate_display_names_in_navigation_providers(repo_directory_input, dry_run=DRY_RUN_SCRIPT)

        print("\nHoàn thành tất cả các tác vụ!")