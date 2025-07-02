import defaultAvatar from '@/assets/user_photo.png';

export const getAvatarUrl = (avatarPath) => {
  if (!avatarPath) return defaultAvatar;

  const encodedPath = encodeURI(avatarPath);
  return `https://localhost:7295${encodedPath}`;
};
