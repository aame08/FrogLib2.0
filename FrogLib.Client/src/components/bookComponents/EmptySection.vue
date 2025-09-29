<script setup>
import { computed } from 'vue';
import { useStore } from 'vuex';

const props = defineProps({
  type: {
    type: String,
    required: true,
    validator: (value) => ['reviews', 'collections'].includes(value),
  },
  count: { type: Number, required: true },
  items: { type: Array, required: true },
  emptyMessage: { type: String, required: true },
  buttonText: { type: String, required: true },
  cardComponent: { type: Object, required: true },
});

const emit = defineEmits(['open-form']);

const store = useStore();

const sectionTitle = computed(() => {
  return {
    reviews: 'рецензий',
    collections: 'подборок',
  }[props.type];
});

const isAuthenticated = computed(() => store.getters['auth/isAuthenticated']);

const handleButtonClick = () => {
  if (props.type === 'reviews') {
    emit('open-form', 'review');
  } else if (props.type === 'collections') {
    emit('open-form', 'collection');
  }
};
</script>

<template>
  <div v-if="count === 0" class="empty-state">
    <p>{{ emptyMessage }}</p>
    <button
      class="btn btn-accent"
      style="margin-top: 15px"
      :disabled="!isAuthenticated"
      @click="handleButtonClick"
    >
      ✧ {{ buttonText }}
    </button>
  </div>
  <div v-else class="cards-container">
    <p class="cards-count">
      Количество {{ sectionTitle }}: {{ count.toFixed(0) }}
    </p>
    <div class="cards-grid">
      <component
        :is="cardComponent"
        v-for="item in items"
        :key="item.id"
        v-bind="item"
      />
    </div>
  </div>
</template>

<style scoped>
.empty-state {
  padding: 40px 0;
  text-align: center;
  color: var(--text-light);
}
.cards-count {
  margin-bottom: 20px;
  font-size: 1.1rem;
  color: var(--text-light);
}
.cards-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 24px;
  width: 100%;
}
</style>
