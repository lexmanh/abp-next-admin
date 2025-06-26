import type { DropMenu } from '../components/Dropdown';
import type { LocaleSetting, LocaleType } from '/#/config';

export const LOCALE: { [key: string]: LocaleType } = {
  ZH_CN: 'zh_CN',
  EN_US: 'en',
  VI_VN: 'vi',
};

export const localeSetting: LocaleSetting = {
  showPicker: true,
  // Locale
  locale: LOCALE.VI_VN,
  // Default locale
  fallback: LOCALE.VI_VN,
  // available Locales
  availableLocales: [LOCALE.VI_VN, LOCALE.EN_US],
};

// locale list
export const localeList: DropMenu[] = [
  {
    text: 'Tiếng Việt',
    event: LOCALE.VI_VN,
  },
  // {
  //   text: '简体中文',
  //   event: LOCALE.ZH_CN,
  // },
  {
    text: 'English',
    event: LOCALE.EN_US,
  },
];
