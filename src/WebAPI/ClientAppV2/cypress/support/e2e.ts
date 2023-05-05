import './commands';
import { basePageSetup } from '@fixtures/baseE2E';
import { MockConfig } from '@mock';

Cypress.Commands.add('basePageSetup', (config: Partial<MockConfig> = {}) => basePageSetup(config));
