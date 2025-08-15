<script setup>
import CollectionCard from '../cards/CollectionCard.vue';

const props = defineProps({
  countCollections: { type: Number, required: true },
  collections: { type: Array, required: true },
});
</script>

<template>
  <div v-if="countCollections === 0" class="empty-state">
    <p>Эта книга пока не добавлена в подборки.</p>
    <button class="btn btn-accent" style="margin-top: 15px">
      Создать подборку
    </button>
  </div>
  <div v-else class="collections-container">
    <p class="collections-count">Количество подборок: {{ countCollections.toFixed(0) }}</p>
    <div class="collections-grid">
      <CollectionCard
        v-for="collection in collections"
        :key="collection.id"
        v-bind="collection"
      />
    </div>
  </div>
</template>

<style scoped>
.empty-state {
  text-align: center;
  padding: 40px 0;
  color: var(--text-light);
}

.collections-count {
  margin-bottom: 20px;
  font-size: 1.1rem;
  color: var(--text-light);
}

.collections-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 24px;
  width: 100%;
}

.btn {
  padding: 8px 16px;
  border-radius: 4px;
  font-weight: 500;
  transition: all 0.2s;
  border: none;
}

.btn-accent {
  background: var(--accent-color);
  color: white;
}

.btn-accent:hover {
  background: var(--hover-color);
  transform: translateY(-2px);
  box-shadow: 0 6px 16px #0000001a;
}
</style>
