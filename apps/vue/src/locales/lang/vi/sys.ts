export default {
  api: {
    operationFailed: 'Thao tác thất bại',
    errorTip: 'Thông báo lỗi',
    errorMessage: 'Thao tác thất bại, hệ thống bất thường!',
    timeoutMessage: 'Đăng nhập quá hạn, vui lòng đăng nhập lại!',
    apiTimeoutMessage: 'Yêu cầu API quá hạn, vui lòng thử lại sau!',
    apiRequestFailed: 'Yêu cầu gặp lỗi, vui lòng thử lại sau',
    networkException: 'Lỗi mạng',
    networkExceptionMsg: 'Lỗi mạng, vui lòng kiểm tra kết nối mạng của bạn!',
    getUserInfoErrorMessage: 'Lấy thông tin người dùng thất bại, vui lòng đăng nhập lại!',

    errMsg401: 'Yêu cầu API cần xác thực, bạn chưa được xác thực hoặc phiên làm việc đã hết hạn, vui lòng đăng nhập lại!',
    errMsg403: 'Người dùng đã được cấp quyền, nhưng quyền truy cập bị từ chối!',
    errMsg404: 'Lỗi yêu cầu mạng, không tìm thấy tài nguyên này!',
    errMsg405: 'Lỗi yêu cầu mạng, phương thức yêu cầu không được phép!',
    errMsg408: 'Yêu cầu mạng quá hạn!',
    errMsg500: 'Lỗi máy chủ, vui lòng liên hệ quản trị viên!',
    errMsg501: 'Chức năng mạng chưa được triển khai!',
    errMsg502: 'Lỗi mạng!',
    errMsg503: 'Dịch vụ không khả dụng, máy chủ tạm thời quá tải hoặc đang bảo trì!',
    errMsg504: 'Mạng quá hạn!',
    errMsg505: 'Phiên bản HTTP không hỗ trợ yêu cầu này!',
  },
  app: {
    logoutTip: 'Thông báo',
    logoutMessage: 'Bạn có chắc chắn muốn thoát khỏi hệ thống?',
    menuLoading: 'Đang tải menu...'
  },
  errorLog: {
    tableTitle: 'Danh sách nhật ký lỗi',
    tableColumnType: 'Loại',
    tableColumnDate: 'Thời gian',
    tableColumnFile: 'Tệp',
    tableColumnMsg: 'Thông báo lỗi',
    tableColumnStackMsg: 'Thông tin stack',

    tableActionDesc: 'Chi tiết',

    modalTitle: 'Chi tiết lỗi',

    fireVueError: 'Nhấn để kích hoạt lỗi Vue',
    fireResourceError: 'Nhấn để kích hoạt lỗi tải tài nguyên',
    fireAjaxError: 'Nhấn để kích hoạt lỗi Ajax',

    enableMessage: 'Chỉ có hiệu lực khi useErrorHandle=true trong `/src/settings/projectSetting.ts`.',
  },
  exception: {
    backLogin: 'Quay lại đăng nhập',
    backHome: 'Quay lại trang chủ',
    subTitle403: 'Xin lỗi, bạn không có quyền truy cập trang này.',
    subTitle404: 'Xin lỗi, trang bạn truy cập không tồn tại.',
    subTitle500: 'Xin lỗi, máy chủ báo lỗi.',
    noDataTitle: 'Trang hiện tại không có dữ liệu',
    networkErrorTitle: 'Lỗi mạng',
    networkErrorSubTitle: 'Xin lỗi, kết nối mạng của bạn đã bị ngắt, vui lòng kiểm tra lại!',
  },
  lock: {
    unlock: 'Nhấn để mở khóa',
    alert: 'Mật khẩu khóa màn hình sai',
    backToLogin: 'Quay lại đăng nhập',
    entry: 'Vào hệ thống',
    placeholder: 'Vui lòng nhập mật khẩu khóa màn hình hoặc mật khẩu người dùng',
  },
  login: {
    backSignIn: 'Quay lại',
    signInFormTitle: 'Đăng nhập',
    mobileSignInFormTitle: 'Đăng nhập bằng di động',
    portalSignInFormTitle: 'Đăng nhập nền tảng',
    qrSignInFormTitle: 'Đăng nhập bằng mã QR',
    signUpFormTitle: 'Đăng ký',
    forgetFormTitle: 'Đặt lại mật khẩu',
    twoFactorFormTitle: 'Xác thực hai yếu tố',

    loginToPortalTitle: 'Đăng nhập vào cổng thông tin',
    signInTitle: 'Hệ thống quản lý back-office sẵn sàng sử dụng',
    signInDesc: 'Nhập thông tin cá nhân của bạn để bắt đầu!',
    policy: 'Tôi đồng ý với chính sách bảo mật xxx',
    scanSign: `Quét mã sau đó nhấn "Xác nhận" để hoàn tất đăng nhập`,

    loginButton: 'Đăng nhập',
    registerButton: 'Đăng ký',
    rememberMe: 'Ghi nhớ tôi',
    forgetPassword: 'Quên mật khẩu?',
    passwordLogin: 'Đăng nhập bằng mật khẩu',
    phoneLogin: 'Đăng nhập bằng mã OTP điện thoại',
    wechatLogin: 'Đăng nhập bằng WeChat',
    ssoLogin: 'Đăng nhập qua SSO',
    otherSignIn: 'Các hình thức đăng nhập khác',

    // notify
    loginSuccessTitle: 'Đăng nhập thành công',
    loginSuccessDesc: 'Chào mừng trở lại',

    // placeholder
    accountPlaceholder: 'Vui lòng nhập tài khoản',
    passwordPlaceholder: 'Vui lòng nhập mật khẩu',
    smsPlaceholder: 'Vui lòng nhập mã xác minh',
    mobilePlaceholder: 'Vui lòng nhập số điện thoại',
    policyPlaceholder: 'Cần đồng ý để đăng ký',
    diffPwd: 'Hai lần nhập mật khẩu không khớp',

    userName: 'Tên đăng nhập', // Hoặc "Tài khoản"
    password: 'Mật khẩu',
    confirmPassword: 'Xác nhận mật khẩu',
    email: 'Email',
    smsCode: 'Mã SMS',
    mobile: 'Số điện thoại',
  },
  abp: {
    remoteServiceNotFound: 'Không tìm thấy định nghĩa dịch vụ có tên {name}!',
    controllerNotFound: 'Không tìm thấy định nghĩa controller có tên {name}!',
    actionNotFound: 'Không tìm thấy định nghĩa phương thức có tên {name}!',
  },
};
