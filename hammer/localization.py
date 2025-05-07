import os
import json
from openai import OpenAI
from pathlib import Path
import time

# --- CẤU HÌNH ---
OPENAI_API_KEY = os.getenv("OPENAI_API_KEY")
OPENAI_MODEL = "gpt-4o" # Hoặc "gpt-3.5-turbo", "gpt-4-turbo"
TARGET_FILENAME = "vi.json"
TARGET_LANGUAGE = "Vietnamese"
API_CALL_DELAY_SECONDS = 2 # Giây đợi giữa các lần gọi API để tránh rate limit
# --- KẾT THÚC CẤU HÌNH ---

def translate_texts_object_openai(texts_object_to_translate: dict, source_lang_description: str) -> dict | None:
    """
    Dịch các giá trị chuỗi trong một dictionary (đại diện cho object "texts").
    Trả về một dictionary đã dịch hoặc None nếu có lỗi.
    """
    if not texts_object_to_translate: # Không có gì để dịch
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
        # Thêm độ trễ trước mỗi lần gọi API thực sự
        if API_CALL_DELAY_SECONDS > 0:
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
                for key_original in texts_object_to_translate: # Đảm bảo chỉ lấy key đã gửi đi
                    if key_original in translated_texts_data:
                        final_translated_texts[key_original] = translated_texts_data[key_original]
                    else:
                        missing_keys_from_model.append(key_original)
                        # Giữ lại giá trị gốc nếu model bỏ sót (hoặc bạn có thể quyết định khác)
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

    print(f"Đang quét kho lưu trữ: {repo_path.resolve()}")
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
        print(f"\nĐang xử lý thư mục: {current_path}")

        # 1. Đọc `vi.json` hiện có (nếu có)
        existing_vi_texts = {}
        existing_vi_full_data = None # Để lưu các metadata khác từ vi.json hiện có
        if target_vi_json_path.is_file():
            print(f"  Tìm thấy tệp đích đã tồn tại: {target_vi_json_path.name}")
            try:
                with open(target_vi_json_path, 'r', encoding='utf-8-sig') as f_vi:
                    existing_vi_full_data = json.load(f_vi)
                    if isinstance(existing_vi_full_data.get("texts"), dict):
                        existing_vi_texts = existing_vi_full_data["texts"]
                        print(f"  Đã tải {len(existing_vi_texts)} bản dịch hiện có từ {target_vi_json_path.name}.")
                    else:
                        print(f"  Cảnh báo: {target_vi_json_path.name} không có key 'texts' hợp lệ hoặc không phải dict. Sẽ tạo mới 'texts'.")
                        existing_vi_full_data = None # Coi như cấu trúc không dùng được
            except Exception as e:
                print(f"  Lỗi khi đọc hoặc parse {target_vi_json_path.name}: {e}. Sẽ tạo mới nếu cần.")
                existing_vi_full_data = None

        # 2. Đọc và hợp nhất các tệp nguồn (en.json, zh-Hans.json)
        merged_source_texts = {}
        en_culture_data_for_metadata = None # Lưu toàn bộ en.json để lấy metadata nếu tạo vi.json mới
        zh_hans_culture_data_for_metadata = None # Tương tự cho zh-Hans

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
                print(f"  Lỗi khi đọc/parse {zh_hans_json_path.name}, bỏ qua texts từ tệp này: {e}")

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
                print(f"  Lỗi khi đọc/parse {en_json_path.name}, bỏ qua texts từ tệp này: {e}")

        merged_source_texts.update(zh_hans_texts_data_temp)
        merged_source_texts.update(en_texts_data_temp) # Tiếng Anh ưu tiên ghi đè

        if not merged_source_texts:
            print(f"  Không có văn bản nguồn nào trong 'texts' để xử lý tại {current_path}.")
            # Nếu vi.json đã tồn tại và không có source text mới, có thể chỉ cần đảm bảo culture là 'vi'
            if existing_vi_full_data is not None:
                existing_vi_full_data["culture"] = "vi"
                if "texts" not in existing_vi_full_data : existing_vi_full_data["texts"] = {} # Đảm bảo có key "texts"
                try:
                    with open(target_vi_json_path, 'w', encoding='utf-8') as f_out:
                        json.dump(existing_vi_full_data, f_out, ensure_ascii=False, indent=2)
                    print(f"  Không có nguồn mới, chỉ cập nhật culture cho {target_vi_json_path.name}.")
                    total_vi_files_created_or_updated +=1
                except Exception as e:
                    print(f"  Lỗi khi ghi lại {target_vi_json_path.name}: {e}")
            continue # Chuyển sang thư mục tiếp theo

        # 3. Xác định các texts thực sự cần dịch
        texts_to_send_for_translation = {}
        for key, source_value in merged_source_texts.items():
            if key not in existing_vi_texts: # Chỉ dịch nếu key chưa có trong vi.json
                texts_to_send_for_translation[key] = source_value

        newly_translated_texts_object = {} # Khởi tạo
        if texts_to_send_for_translation:
            print(f"  Tìm thấy {len(texts_to_send_for_translation)} texts mới/chưa được dịch cần gửi đến API.")
            total_texts_actually_sent_to_api += len(texts_to_send_for_translation)

            source_lang_description = ""
            if has_en and has_zh_hans:
                source_lang_description = "primarily English, with potential Simplified Chinese phrases"
            elif has_en:
                source_lang_description = "English"
            elif has_zh_hans:
                source_lang_description = "Simplified Chinese"

            # Gọi API để dịch các texts mới
            temp_translated = translate_texts_object_openai(texts_to_send_for_translation, source_lang_description)
            if temp_translated:
                newly_translated_texts_object = temp_translated
            else:
                print(f"  Dịch thuật API không thành công cho các văn bản mới tại {current_path}.")
        else:
            print(f"  Không có văn bản mới nào cần dịch cho {current_path}. Tất cả các khóa nguồn đã có trong {target_vi_json_path.name}.")

        # 4. Hợp nhất bản dịch mới với bản dịch hiện có
        final_combined_vi_texts = existing_vi_texts.copy() # Bắt đầu với các bản dịch cũ
        final_combined_vi_texts.update(newly_translated_texts_object) # Thêm/cập nhật các bản dịch mới

        # 5. Xây dựng và lưu tệp vi.json cuối cùng
        final_vi_json_structure = {}
        # Ưu tiên giữ lại metadata từ vi.json hiện có nếu nó hợp lệ
        if existing_vi_full_data is not None:
            final_vi_json_structure = {k: v for k, v in existing_vi_full_data.items() if k != "texts"}
        # Nếu vi.json không tồn tại hoặc không hợp lệ, lấy metadata từ en.json (ưu tiên) hoặc zh-Hans.json
        elif en_culture_data_for_metadata:
            final_vi_json_structure = {k: v for k, v in en_culture_data_for_metadata.items() if k != "texts"}
        elif zh_hans_culture_data_for_metadata:
            final_vi_json_structure = {k: v for k, v in zh_hans_culture_data_for_metadata.items() if k != "texts"}

        final_vi_json_structure["culture"] = "vi" # Đảm bảo culture là "vi"
        final_vi_json_structure["texts"] = final_combined_vi_texts # Gán texts đã hợp nhất

        try:
            with open(target_vi_json_path, 'w', encoding='utf-8') as f_out:
                json.dump(final_vi_json_structure, f_out, ensure_ascii=False, indent=2)
            print(f"  Đã cập nhật/lưu thành công tệp: {target_vi_json_path}")
            total_vi_files_created_or_updated += 1
        except Exception as e:
            print(f"  Lỗi khi ghi tệp {target_vi_json_path}: {e}")

    print(f"\n--- Hoàn tất xử lý ---")
    print(f"Đã xử lý {total_source_sets_processed} bộ tệp nguồn (en.json và/hoặc zh-Hans.json).")
    print(f"Đã gửi tổng cộng {total_texts_actually_sent_to_api} cặp key-value mới/chưa dịch đến API.")
    print(f"Đã tạo/cập nhật thành công {total_vi_files_created_or_updated} tệp {TARGET_FILENAME}.")

if __name__ == "__main__":
    if not OPENAI_API_KEY:
        print("Vui lòng đặt biến môi trường OPENAI_API_KEY trước khi chạy script.")
    else:
        repo_directory_input = input("Nhập đường dẫn đến thư mục kho lưu trữ của bạn (để trống nếu là thư mục hiện tại): ").strip()
        if not repo_directory_input:
            repo_directory_input = "."
        process_repo(repo_directory_input)