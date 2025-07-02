import dayjs from 'dayjs';
import 'dayjs/locale/ru';

dayjs.locale('ru');

export const formattedDate = (date) => {
  return dayjs(date).isValid()
    ? dayjs(date).format('DD MMMM YYYY')
    : 'Неверный формат даты';
};