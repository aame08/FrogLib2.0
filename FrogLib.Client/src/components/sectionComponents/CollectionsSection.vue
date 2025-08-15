<script setup>
import { ref, computed } from 'vue';
import CollectionCard from '../cards/CollectionCard.vue';

const props = defineProps({
  collections: { type: Array, required: true },
});

const currentIndex = ref(0);

const visibleCollections = computed(() => {
  return props.collections.slice(currentIndex.value, currentIndex.value + 3);
});

const isPrevDisabled = computed(() => {
  return currentIndex.value === 0;
});

const isNextDisabled = computed(() => {
  return currentIndex.value + 5 >= props.collections.length;
});

const prevSlide = () => {
  if (!isPrevDisabled.value) {
    currentIndex.value -= 1;
  }
};

const nextSlide = () => {
  if (!isNextDisabled.value) {
    currentIndex.value += 1;
  }
};
</script>

<template>
  <section class="section">
    <div class="section-title">
      <h2>✧ Популярные подборки ✧</h2>
    </div>
    <div class="collections-container-wrapper">
      <button
        class="slide-buttons prev-button"
        @click="prevSlide"
        :disabled="isPrevDisabled"
      >
        ←
      </button>
      <div class="collections-container">
        <div class="collections-grid">
          <CollectionCard
            v-for="(collection, index) in visibleCollections"
            :key="index"
            :id="collection.id"
            :title="collection.title"
            :description="collection.description"
            :books="collection.books"
            :created-date="collection.createdDate"
            :user-name="collection.userName"
            :user-u-r-l="collection.userURL"
          />
        </div>
      </div>
      <button
        class="slide-buttons next-button"
        @click="nextSlide"
        :disabled="isNextDisabled"
      >
        →
      </button>
    </div>
  </section>
</template>

<style scoped>
.collections-container-wrapper {
  position: relative;
  display: flex;
  align-items: center;
  gap: 20px;
  padding: 0 20px;
}

.collections-container {
  position: relative;
  width: 100%;
  overflow: hidden;
  flex-grow: 1;
}

.collections-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1.5rem;
  padding: 10px 0;
  scroll-behavior: smooth;
}
</style>
