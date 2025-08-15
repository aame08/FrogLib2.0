<script setup>
import { pluralize } from '@/utils/pluralize';

const props = defineProps({
  isbN13: { type: String, required: true },
  description: { type: String, required: true },
  yearPublication: { type: Number, required: true },
  pageCount: { type: Number, required: true },
  languageBook: { type: String, required: true },
  category: { type: String, required: true },
  publisher: { type: String, required: true },
  ratingStats: { type: Object, required: true },
  bookmarkStats: { type: Object, required: true },
});
</script>

<template>
  <section class="section">
    <h3 class="section-title">Описание</h3>
    <p class="book-description">{{ description }}</p>
  </section>

  <section class="section">
    <h3 class="section-title">Детали</h3>
    <div class="details-grid">
      <div class="detail-item">
        <div class="detail-label">ISBN-13</div>
        <div>{{ isbN13 }}</div>
      </div>
      <div class="detail-item">
        <div class="detail-label">Издатель</div>
        <div>{{ publisher }}</div>
      </div>
      <div class="detail-item">
        <div class="detail-label">Год издания</div>
        <div>{{ yearPublication }}</div>
      </div>
      <div class="detail-item">
        <div class="detail-label">Категория</div>
        <div>{{ category }}</div>
      </div>
      <div class="detail-item">
        <div class="detail-label">Количество страниц</div>
        <div>{{ pageCount }}</div>
      </div>
      <div class="detail-item">
        <div class="detail-label">Язык книги</div>
        <div>{{ languageBook }}</div>
      </div>
    </div>
  </section>

  <section class="section">
    <h3 class="section-title">Статистика</h3>
    <div class="stats-container">
      <div class="stat-card">
        <div class="stat-header">
          <h4 class="stat-title">Оценки</h4>
          <div class="stat-value">
            {{ ratingStats.averageRating.toFixed(1) }} ({{
              ratingStats.totalRatings.toFixed(0)
            }})
          </div>
        </div>

        <div v-for="rating in ratingStats.ratingDistribution" class="stat-row">
          <div class="stat-label">{{ rating.ratingValue }}</div>
          <div class="stat-bar" style="flex: 1">
            <div
              class="stat-fill"
              :style="{ width: rating.percentage + '%' }"
            ></div>
          </div>
          <div class="stat-percent">{{ rating.percentage.toFixed(0) }}%</div>
          <div class="stat-count">{{ rating.count }}</div>
        </div>
      </div>

      <div class="stat-card">
        <div class="stat-header">
          <h4 class="stat-title">В списках у</h4>
          <div class="stat-value">
            {{ bookmarkStats.totalBookmarks }}
            {{ pluralize(bookmarkStats.totalBookmarks, 'пользователь') }}
          </div>
        </div>

        <div
          v-for="bookmark in bookmarkStats.bookmarkDistribution"
          class="stat-row"
        >
          <div class="stat-label" style="width: 100px">
            {{ bookmark.listType }}
          </div>
          <div class="stat-bar" style="flex: 1">
            <div
              class="stat-fill"
              :style="{ width: bookmark.percentage + '%' }"
            ></div>
          </div>
          <div class="stat-percent">{{ bookmark.percentage.toFixed(0) }}%</div>
          <div class="stat-count">{{ bookmark.count }}</div>
        </div>
      </div>
    </div>
  </section>
</template>

<style scoped>
.section {
  margin: 0;
  padding: 0;
  margin-bottom: 30px;
}

.section-title {
  font-size: 1.4rem;
  margin-bottom: 15px;
  padding-bottom: 8px;
  border-bottom: 2px solid var(--accent-color);
  display: inline-block;
}

.section-title::after {
  display: none !important;
}

.book-description {
  font-size: 1.05rem;
  line-height: 1.7;
}

.details-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 15px;
}

.detail-item {
  font-size: 0.95rem;
  background: #ffffff;
  padding: 12px;
  border-radius: 4px;
  box-shadow: 0 4px 12px #0000000d;
}

.detail-label {
  color: var(--text-light);
  margin-bottom: 4px;
  font-size: 0.9rem;
}

.stats-container {
  display: grid;
  grid-template-rows: repeat(auto-fit, minmax(300px, 1fr));
  gap: 30px;
  margin-top: 40px;
}

.stat-card {
  background: #ffffff;
  padding: 20px;
  border-radius: 6px;
  box-shadow: 0 4px 12px #0000000d;
  border-top: 3px solid var(--accent-color);
}

.stat-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 15px;
}

.stat-title {
  font-size: 1.1rem;
  font-weight: 600;
}

.stat-value {
  font-weight: 600;
  color: var(--hover-color);
}

.stat-bar {
  height: 8px;
  background: #f0f0f0;
  border-radius: 4px;
  margin: 8px 0;
  overflow: hidden;
}

.stat-fill {
  height: 100%;
  background: linear-gradient(90deg, var(--hover-color), var(--accent-color));
  border-radius: 4px;
}

.stat-row {
  display: flex;
  align-items: center;
  margin-bottom: 10px;
}

.stat-label {
  width: 30px;
  font-weight: 500;
}

.stat-percent {
  width: 50px;
  text-align: right;
  color: var(--text-light);
  font-size: 0.9rem;
}

.stat-count {
  width: 40px;
  text-align: right;
  color: var(--dark-text);
  font-size: 0.9rem;
}
</style>
