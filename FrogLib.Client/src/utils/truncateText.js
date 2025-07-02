export function truncateText(
  text,
  maxLength = 100,
  placeholder = 'Нет описания.'
) {
  if (!text || text.trim() === '') {
    return placeholder;
  }
  return text.length > maxLength ? text.slice(0, maxLength) + '...' : text;
}
