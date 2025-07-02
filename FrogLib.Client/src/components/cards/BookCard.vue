<script setup>
const props = defineProps({
  id: { type: Number, required: true },
  title: { type: String, required: true },
  imageURL: { type: String, required: true },
  averageRating: { type: Number, required: true },
});
</script>

<template>
  <div class="book-card">
    <div class="book-rating">★ {{ averageRating.toFixed(1) }}</div>
    <div class="book-cover">
      <img :src="imageURL" :alt="title" />
    </div>
    <div style="padding: 0.5rem"></div>
  </div>
</template>

<style scoped>
.book-card {
  flex: 0 0 calc(20% - 1.2rem);
  min-width: 180px;
  background-color: #ffffff;
  border-radius: 6px;
  overflow: hidden;
  box-shadow: 0 2px 5px #0000001a;
  scroll-snap-align: center;
  transition: transform 0.3s, box-shadow 0.3s;
}

.book-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 5px 15px #0000001a;
}

.book-rating {
  color: var(--accent-color);
  font-weight: bold;
}

.book-cover {
  display: flex;
  align-items: center;
  justify-content: center;
}

.book-cover img {
  height: 250px;
}
</style>
