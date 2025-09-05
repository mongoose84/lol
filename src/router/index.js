import { createRouter, createWebHistory } from 'vue-router'
import ChampionStats from '../views/ChampionStats.vue'
import Search from '../views/Search.vue'

const routes = [
  {
    path: '/',
    name: 'Search',
    component: Search,
  },
  {
    path: '/championstats',
    name: 'ChampionStats',
    component: ChampionStats,

    // pass the query string as a prop so the component can read it easily
    props: route => ({ champion: route.query.champion, region: route.query.region }),
  }
]

export default createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
})