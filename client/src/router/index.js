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
    props: route => ({ gameName: route.query.gameName, tagLine: route.query.tagLine }),
  }
]

export default createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
})