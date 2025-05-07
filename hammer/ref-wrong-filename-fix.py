import os
import re
import subprocess

# Các phần mở rộng tệp dự án .NET phổ biến
PROJECT_FILE_EXTENSIONS = ('.csproj', '.vbproj', '.fsproj')
# Các phần mở rộng tệp giải pháp .NET phổ biến (có thể chứa đường dẫn tệp)
SOLUTION_FILE_EXTENSIONS = ('.sln',)
# Các phần mở rộng tệp thường được tham chiếu
# Bạn có thể mở rộng danh sách này nếu cần
REFERENCED_FILE_EXTENSIONS = (
    '.cs', '.vb', '.fs', '.xaml', '.xml', '.config', '.json', '.png', '.jpg', '.jpeg',
    '.gif', '.resx', '.settings', '.cshtml', '.js', '.css', '.dll', '.targets', '.props',
    '.ico', '.txt', '.md'
)

def get_actual_filename(directory, filename_to_check):
    """
    Lấy tên tệp thực tế (phân biệt chữ hoa chữ thường) trong một thư mục.
    Trả về tên tệp thực tế nếu tìm thấy, nếu không trả về None.
    """
    try:
        for f_name in os.listdir(directory):
            if f_name.lower() == filename_to_check.lower():
                return f_name
    except FileNotFoundError:
        pass
    return None

def fix_casing_in_file_content(file_path, base_dir):
    """
    Đọc nội dung của một tệp, tìm kiếm các tham chiếu tệp và sửa lỗi phân biệt chữ hoa chữ thường.
    Trả về: (số_lượng_sửa_lỗi, trạng_thái_lỗi)
    trạng_thái_lỗi có thể là "READ_ERROR", "WRITE_ERROR", hoặc None.
    """
    corrections_count = 0
    error_status = None

    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
    except Exception as e:
        print(f"LỖI: Không thể đọc tệp {file_path}: {e}")
        return 0, "READ_ERROR"

    original_content = content
    lines = original_content.splitlines()
    new_lines = []

    path_regex = re.compile(
        r'(?P<prefix>(?:Include|Update|Content|Link|Source|Code|HintPath|Condition|Path|Url|file|href|src)\s*=\s*["\'](?:(?:\.\.[/\\])*))'
        r'(?P<path>(?:[^"\'/\\]+[/\\])*?)'
        r'(?P<filename>[^"\'/\\]+\.(?P<extension>' + '|'.join(ext.lstrip('.') for ext in REFERENCED_FILE_EXTENSIONS) + '))'
                                                                                                                       r'(?P<suffix>["\'])',
        re.IGNORECASE
    )
    project_file_tag_regex = re.compile(
        r'<(?P<tag>Compile|Content|None|EmbeddedResource|Page|ApplicationDefinition|Resource|Reference|ProjectReference|COMReference|NativeReference|TypeScriptCompile|Analyzer|AdditionalFiles|Xsd|WCFMetadataStorage|EntityDeploy|CodeAnalysisDictionary|CodeAnalysisImport|Import|Target|ItemGroup|PropertyGroup)\s+(?P<attribute_name>Include|Update|HintPath|AssemblyFile|Name)\s*=\s*"(?P<filepath>[^"]+)"',
        re.IGNORECASE
    )
    sln_project_regex = re.compile(
        r'Project\s*\([^)]+\)\s*=\s*"[^"]+",\s*"(?P<project_path>[^"]+\.(?:csproj|vbproj|fsproj))",\s*"[^"]+"',
        re.IGNORECASE
    )

    is_project_or_solution = file_path.endswith(PROJECT_FILE_EXTENSIONS + SOLUTION_FILE_EXTENSIONS)

    for line_number, line in enumerate(lines):
        new_line = line
        # --- Xử lý các thẻ cụ thể trong tệp dự án ---
        if is_project_or_solution:
            # (Giữ nguyên logic xử lý thẻ project_file_tag_regex và sln_project_regex)
            # Khi một sửa lỗi được thực hiện trong các vòng lặp này:
            # Ví dụ: new_line = new_line.replace(...)
            #        corrections_count += 1
            # --- (Bắt đầu logic project_file_tag_regex) ---
            for match in project_file_tag_regex.finditer(line):
                original_path_in_file = match.group('filepath')
                if not any(original_path_in_file.lower().endswith(ext) for ext in REFERENCED_FILE_EXTENSIONS + PROJECT_FILE_EXTENSIONS):
                    continue

                normalized_path = original_path_in_file.replace('\\', '/')
                path_parts = normalized_path.split('/')
                filename_in_file = path_parts[-1]
                relative_dir_parts = path_parts[:-1]

                current_check_dir = base_dir
                if relative_dir_parts:
                    current_check_dir = os.path.normpath(os.path.join(base_dir, *relative_dir_parts))

                actual_filename = get_actual_filename(current_check_dir, filename_in_file)

                if actual_filename and actual_filename != filename_in_file:
                    corrected_path_parts = relative_dir_parts + [actual_filename]
                    original_separator = '\\' if '\\' in original_path_in_file else '/'
                    corrected_path_in_file = original_separator.join(corrected_path_parts)

                    print(f"  SỬA LỖI TRONG '{file_path}' (Dòng {line_number + 1}):")
                    print(f"    Sai: '{original_path_in_file}'")
                    print(f"    Đúng: '{corrected_path_in_file}'")
                    new_line = new_line.replace(f'"{original_path_in_file}"', f'"{corrected_path_in_file}"')
                    corrections_count += 1
            # --- (Kết thúc logic project_file_tag_regex) ---

            # --- (Bắt đầu logic sln_project_regex) ---
            if file_path.endswith(SOLUTION_FILE_EXTENSIONS):
                for match in sln_project_regex.finditer(line): # Tìm trên line gốc, vì new_line có thể đã thay đổi
                    original_path_in_file = match.group('project_path')
                    # ... (logic tương tự để tìm actual_filename)
                    normalized_path = original_path_in_file.replace('\\', '/')
                    path_parts = normalized_path.split('/')
                    filename_in_file = path_parts[-1]
                    relative_dir_parts = path_parts[:-1]

                    current_check_dir = os.path.dirname(file_path)
                    if relative_dir_parts:
                        current_check_dir = os.path.normpath(os.path.join(current_check_dir, *relative_dir_parts))

                    actual_filename = get_actual_filename(current_check_dir, filename_in_file)

                    if actual_filename and actual_filename != filename_in_file:
                        corrected_path_parts = relative_dir_parts + [actual_filename]
                        original_separator = '\\' if '\\' in original_path_in_file else '/'
                        corrected_path_in_file = original_separator.join(corrected_path_parts)

                        print(f"  SỬA LỖI TRONG '{file_path}' (Dòng {line_number + 1}):")
                        print(f"    Sai: '{original_path_in_file}' (tham chiếu dự án trong .sln)")
                        print(f"    Đúng: '{corrected_path_in_file}'")
                        pattern_to_replace = f'"{re.escape(original_path_in_file)}"'
                        replacement_string = f'"{corrected_path_in_file}"'
                        # Áp dụng thay thế vào new_line, không phải line gốc
                        temp_line_before_sln_change = new_line
                        new_line = re.sub(pattern_to_replace, replacement_string, new_line)
                        if new_line != temp_line_before_sln_change:
                            corrections_count +=1
            # --- (Kết thúc logic sln_project_regex) ---

        # --- Xử lý các đường dẫn tệp chung ---
        # (Giữ nguyên logic xử lý path_regex)
        # Khi một sửa lỗi được thực hiện:
        # Ví dụ: new_line = new_line.replace(...)
        #        corrections_count += 1
        # --- (Bắt đầu logic path_regex) ---
        # Áp dụng path_regex cho new_line hiện tại (có thể đã được sửa đổi bởi các regex ở trên)
        # Tạo một bản sao của new_line để thực hiện các thay thế lặp đi lặp lại mà không ảnh hưởng đến vòng lặp finditer
        line_to_search_for_general_paths = new_line
        updated_new_line_segment = ""
        last_match_end = 0

        for match in path_regex.finditer(line_to_search_for_general_paths):
            original_full_match = match.group(0)
            prefix = match.group('prefix')
            path_segment = match.group('path')
            filename_in_file = match.group('filename')
            suffix = match.group('suffix')

            current_dir_of_file_being_scanned = os.path.dirname(file_path)
            referenced_file_dir_parts = path_segment.replace('\\', '/').rstrip('/').split('/')
            referenced_file_dir_parts = [part for part in referenced_file_dir_parts if part]

            temp_current_dir = current_dir_of_file_being_scanned
            num_parent_dirs = prefix.count('../') + prefix.count('..\\')
            for _ in range(num_parent_dirs):
                temp_current_dir = os.path.dirname(temp_current_dir)

            if referenced_file_dir_parts:
                check_dir_for_actual_filename = os.path.normpath(os.path.join(temp_current_dir, *referenced_file_dir_parts))
            else:
                check_dir_for_actual_filename = temp_current_dir

            actual_filename = get_actual_filename(check_dir_for_actual_filename, filename_in_file)

            # Thêm phần không khớp vào new_line
            updated_new_line_segment += line_to_search_for_general_paths[last_match_end:match.start()]

            if actual_filename and actual_filename != filename_in_file:
                incorrect_sub_path = f"{path_segment}{filename_in_file}"
                correct_sub_path = f"{path_segment}{actual_filename}"
                corrected_full_match = f"{prefix}{correct_sub_path}{suffix}"

                print(f"  SỬA LỖI TRONG '{file_path}' (Dòng {line_number + 1}):")
                print(f"    Sai (tham chiếu chung): '{incorrect_sub_path}' trong '{original_full_match}'")
                print(f"    Đúng: '{correct_sub_path}'")

                updated_new_line_segment += corrected_full_match
                corrections_count += 1
            else:
                # Không có thay đổi, thêm lại phần khớp gốc
                updated_new_line_segment += original_full_match
            last_match_end = match.end()

        # Thêm phần còn lại của dòng (sau lần khớp cuối cùng)
        updated_new_line_segment += line_to_search_for_general_paths[last_match_end:]
        new_line = updated_new_line_segment # Cập nhật new_line với tất cả các thay đổi từ path_regex
        # --- (Kết thúc logic path_regex) ---

        new_lines.append(new_line)

    if corrections_count > 0:
        try:
            with open(file_path, 'w', encoding='utf-8', newline='\n') as f:
                f.write('\n'.join(new_lines))
            print(f"Đã cập nhật tệp: {file_path}")
        except Exception as e:
            print(f"LỖI: Không thể ghi vào tệp {file_path}: {e}")
            error_status = "WRITE_ERROR"
            # Trả về số lượng sửa lỗi đã cố gắng thực hiện, ngay cả khi ghi bị lỗi
            return corrections_count, error_status

    return corrections_count, error_status


def fix_filenames_on_disk(repo_path):
    """
    Trả về: (số_lần_đổi_tên_thành_công, số_lỗi_git_mv, lỗi_git_ls_tree_hay_không)
    """
    print("\nKiểm tra tên tệp trên đĩa so với Git HEAD...")
    successful_renames = 0
    git_mv_errors = 0
    git_ls_tree_error = False

    try:
        result = subprocess.run(['git', 'ls-tree', '-r', '--name-only', 'HEAD'], cwd=repo_path, capture_output=True, text=True, check=True)
        git_tracked_files = result.stdout.splitlines()
    except subprocess.CalledProcessError as e:
        print(f"LỖI: Không thể lấy danh sách tệp từ Git (ls-tree): {e}")
        git_ls_tree_error = True
        return 0, 0, git_ls_tree_error
    except Exception as e:
        print(f"LỖI không mong muốn khi chạy 'git ls-tree': {e}")
        git_ls_tree_error = True
        return 0, 0, git_ls_tree_error

    files_to_rename_on_disk = {}
    for root, dirs, files in os.walk(repo_path, topdown=True):
        if '.git' in dirs: dirs.remove('.git')
        if '.git' in root.split(os.sep): continue

        all_items_on_disk = dirs + files
        for item_on_disk in all_items_on_disk:
            full_path_on_disk = os.path.join(root, item_on_disk)
            relative_path_on_disk = os.path.relpath(full_path_on_disk, repo_path).replace('\\', '/')

            for git_file_path in git_tracked_files:
                if relative_path_on_disk.lower() == git_file_path.lower() and relative_path_on_disk != git_file_path:
                    actual_git_full_path = os.path.join(repo_path, git_file_path.replace('/', os.sep))
                    if os.path.exists(full_path_on_disk) and not os.path.exists(actual_git_full_path):
                        files_to_rename_on_disk[full_path_on_disk] = actual_git_full_path
                    break

    if files_to_rename_on_disk:
        print("Tìm thấy các tệp/thư mục trên đĩa có trường hợp khác với Git HEAD. Đang cố gắng đổi tên:")
        for disk_path, git_tracked_path_target in files_to_rename_on_disk.items():
            try:
                target_dir = os.path.dirname(git_tracked_path_target)
                if not os.path.exists(target_dir):
                    os.makedirs(target_dir, exist_ok=True)

                if os.path.exists(disk_path):
                    print(f"  Đổi tên trên đĩa: '{disk_path}' -> '{git_tracked_path_target}'")
                    relative_disk_path = os.path.relpath(disk_path, repo_path)
                    relative_git_path_target = os.path.relpath(git_tracked_path_target, repo_path)

                    subprocess.run(['git', 'mv', '-f', relative_disk_path, relative_git_path_target], cwd=repo_path, check=True, capture_output=True, text=True)
                    print(f"    Đã sử dụng 'git mv -f \"{relative_disk_path}\" \"{relative_git_path_target}\"'")
                    successful_renames += 1
                else:
                    print(f"    CẢNH BÁO: Tệp nguồn '{disk_path}' không tìm thấy để đổi tên.")
            except subprocess.CalledProcessError as e:
                print(f"    LỖI khi chạy 'git mv' cho '{disk_path}' -> '{git_tracked_path_target}': {e.stderr or e}")
                git_mv_errors += 1
            except Exception as e:
                print(f"    LỖI không mong muốn khi đổi tên '{disk_path}': {e}")
                git_mv_errors += 1
    else:
        print("Không tìm thấy sự không nhất quán về trường hợp giữa tệp trên đĩa và Git HEAD cần sửa trực tiếp trên đĩa.")

    return successful_renames, git_mv_errors, git_ls_tree_error


def process_repository(repo_path):
    """
    Xử lý tất cả các tệp trong kho lưu trữ và thu thập số liệu thống kê.
    """
    print(f"Đang xử lý kho lưu trữ: {repo_path}")

    summary_stats = {
        "files_checked_for_content": 0,
        "files_with_content_changes": 0,
        "total_reference_fixes_in_content": 0,
        "content_read_errors": 0,
        "content_write_errors": 0,
        "disk_renames_successful": 0,
        "disk_rename_git_mv_errors": 0,
        "disk_renames_git_ls_error": False
    }

    print("\n----- BƯỚC 1: Sửa lỗi tham chiếu trong nội dung tệp -----")
    for root, dirs, files in os.walk(repo_path):
        if '.git' in dirs: dirs.remove('.git')
        if '.git' in root.split(os.sep): continue

        for filename in files:
            file_path = os.path.join(root, filename)
            if filename.endswith(PROJECT_FILE_EXTENSIONS + SOLUTION_FILE_EXTENSIONS) or \
                    filename.endswith(('.xml', '.config', '.json', '.targets', '.props')):
                print(f"\nKiểm tra tệp: {file_path}")
                summary_stats["files_checked_for_content"] += 1
                base_dir_for_refs = os.path.dirname(file_path)

                corrections, error_status = fix_casing_in_file_content(file_path, base_dir_for_refs)

                if error_status == "READ_ERROR":
                    summary_stats["content_read_errors"] += 1
                elif error_status == "WRITE_ERROR":
                    summary_stats["content_write_errors"] += 1

                if corrections > 0:
                    summary_stats["files_with_content_changes"] += 1
                    summary_stats["total_reference_fixes_in_content"] += corrections

    if summary_stats["files_checked_for_content"] == 0:
        print("Không tìm thấy tệp nào phù hợp để quét nội dung.")
    elif summary_stats["total_reference_fixes_in_content"] > 0:
        print(f"\nĐã hoàn tất Bước 1. Tổng cộng {summary_stats['total_reference_fixes_in_content']} lỗi tham chiếu đã được sửa trong {summary_stats['files_with_content_changes']} tệp.")
    else:
        print("\nĐã hoàn tất Bước 1. Không tìm thấy lỗi tham chiếu nào cần sửa trong nội dung các tệp đã quét.")


    print("\n----- BƯỚC 2: Đồng bộ hóa tên tệp trên đĩa với Git HEAD (nếu cần) -----")
    s_renames, mv_errors, ls_error = fix_filenames_on_disk(repo_path)
    summary_stats["disk_renames_successful"] = s_renames
    summary_stats["disk_rename_git_mv_errors"] = mv_errors
    summary_stats["disk_renames_git_ls_error"] = ls_error

    # --- In Bảng Tóm Tắt ---
    print("\n\n============================================")
    print("           BẢNG TỔNG KẾT KẾT QUẢ          ")
    print("============================================")

    print("\n--- Sửa lỗi tham chiếu trong nội dung tệp ---")
    print(f"Số tệp đã quét nội dung:                 {summary_stats['files_checked_for_content']}")
    print(f"Số tệp có nội dung được sửa đổi:          {summary_stats['files_with_content_changes']}")
    print(f"Tổng số lỗi tham chiếu trong nội dung đã sửa: {summary_stats['total_reference_fixes_in_content']}")
    if summary_stats['content_read_errors'] > 0:
        print(f"  Lỗi đọc tệp (không thể quét):         {summary_stats['content_read_errors']}")
    if summary_stats['content_write_errors'] > 0:
        print(f"  Lỗi ghi tệp (sau khi sửa):            {summary_stats['content_write_errors']}")

    print("\n--- Đồng bộ hóa tên tệp trên đĩa với Git HEAD ---")
    if summary_stats['disk_renames_git_ls_error']:
        print("  Lỗi khi lấy danh sách tệp từ Git (git ls-tree). Không thể thực hiện đồng bộ hóa.")
    else:
        print(f"Số tệp/thư mục được đổi tên trên đĩa (git mv): {summary_stats['disk_renames_successful']}")
        if summary_stats['disk_rename_git_mv_errors'] > 0:
            print(f"  Số lỗi khi thực hiện 'git mv':          {summary_stats['disk_rename_git_mv_errors']}")

    print("============================================")

    total_changes_made = summary_stats['total_reference_fixes_in_content'] + summary_stats['disk_renames_successful']
    total_errors_encountered = (summary_stats['content_read_errors'] +
                                summary_stats['content_write_errors'] +
                                summary_stats['disk_rename_git_mv_errors'] +
                                (1 if summary_stats['disk_renames_git_ls_error'] else 0))

    if total_changes_made > 0:
        print("\nVui lòng xem xét kỹ các thay đổi (ví dụ: 'git status', 'git diff')")
        print("và commit nếu mọi thứ đều ổn.")

    if total_errors_encountered > 0:
        print("\nCẢNH BÁO: Đã xảy ra lỗi trong quá trình thực thi. Hãy kiểm tra log chi tiết ở trên.")
        if summary_stats['content_write_errors'] > 0 or summary_stats['disk_rename_git_mv_errors'] > 0 :
            print("  Đặc biệt lưu ý các lỗi ghi tệp hoặc lỗi 'git mv'.")

    if total_changes_made == 0 and total_errors_encountered == 0:
        print("\nKhông có thay đổi nào được thực hiện và không có lỗi nào xảy ra.")


if __name__ == "__main__":
    repo_directory = "../"
    # repo_directory = "/path/to/your/dotnet/repo"

    abs_repo_path = os.path.abspath(repo_directory)

    if not os.path.isdir(abs_repo_path):
        print(f"LỖI: Thư mục kho lưu trữ không tồn tại: {abs_repo_path}")
    elif not os.path.isdir(os.path.join(abs_repo_path, '.git')):
        print(f"LỖI: Thư mục được cung cấp không phải là một kho lưu trữ Git hợp lệ: {abs_repo_path}")
        print("Tập lệnh này được thiết kế để hoạt động bên trong một kho lưu trữ Git.")
    else:
        process_repository(abs_repo_path)