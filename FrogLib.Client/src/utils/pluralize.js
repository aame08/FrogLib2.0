export function pluralize(count, word) {
  const words = {
    просмотр: ['просмотр', 'просмотра', 'просмотров'],
    комментарий: ['комментарий', 'комментария', 'комментариев'],
    пользователь: ['пользователя', 'пользователей'],
    книга: ['книга', 'книги', 'книг'],
  };

  if (!words[word]) return word;

  if (word === 'пользователь') {
    return count === 1 ? words[word][0] : words[word][1];
  }

  if (words[word].length === 2) {
    return count === 1
      ? word
      : count % 100 >= 5 && count % 100 <= 20
      ? words[word][1]
      : [2, 3, 4].includes(count % 10)
      ? words[word][0]
      : words[word][1];
  }

  const cases = [2, 0, 1, 1, 1, 2];
  const index =
    count % 100 > 4 && count % 100 < 20 ? 2 : cases[Math.min(count % 10, 5)];

  return words[word][index];
}
