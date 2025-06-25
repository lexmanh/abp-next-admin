export interface GrowCardItem {
  icon: string;
  title: string;
  value: number;
  total: number;
  color: string;
  action: string;
}

export const growCardList: GrowCardItem[] = [
  {
    title: 'Số lượt truy cập', 
    icon: 'visit-count|svg',
    value: 2000,
    total: 120000,
    color: 'green',
    action: 'Tháng',
  },
  {
    title: 'Số lượng bán hàng',
    icon: 'total-sales|svg',
    value: 20000,
    total: 500000,
    color: 'blue',
    action: 'Tháng',
  },
  {
    title: 'Số lượt tải xuống',
    icon: 'download-count|svg',
    value: 8000,
    total: 120000,
    color: 'orange',
    action: 'Tuần',
  },
  {
    title: 'Số lượng giao dịch',
    icon: 'transaction|svg',
    value: 5000,
    total: 50000,
    color: 'purple',
    action: 'Năm',
  },
];
