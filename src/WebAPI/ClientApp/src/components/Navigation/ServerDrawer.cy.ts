import ServerDrawer from './ServerDrawer.vue'

describe('<ServerDrawer />', () => {
  it('renders', () => {
    // see: https://test-utils.vuejs.org/guide/
    cy.mount(ServerDrawer)
  })
})