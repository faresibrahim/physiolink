import { defineConfig, devices } from '@playwright/test';
import { ADMIN_STATE } from './tests/helpers';

// Local credentials live in e2e/.env (gitignored); see .env.example.
try {
  process.loadEnvFile('.env');
} catch {
  // No .env file: fall back to whatever is already in the environment.
}

const ADMIN_URL = process.env.E2E_ADMIN_URL ?? 'http://localhost:5001';
const API_URL = process.env.E2E_API_URL ?? 'http://localhost:5218';

export default defineConfig({
  testDir: './tests',
  // The suite shares one local database; run serially so tests don't race each other.
  fullyParallel: false,
  workers: 1,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 1 : 0,
  reporter: [['list'], ['html', { open: 'never' }]],

  use: {
    baseURL: ADMIN_URL,
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
  },

  projects: [
    { name: 'setup', testMatch: /.*\.setup\.ts/ },
    {
      // Tests that start signed out (language gate, login form).
      name: 'anonymous',
      testMatch: /anonymous\/.*\.spec\.ts/,
      use: { ...devices['Desktop Chrome'] },
    },
    {
      // Tests that reuse the admin session saved by auth.setup.ts.
      name: 'admin',
      testMatch: /admin\/.*\.spec\.ts/,
      dependencies: ['setup'],
      use: { ...devices['Desktop Chrome'], storageState: ADMIN_STATE },
    },
  ],

  // Start the API and admin panel if they aren't already running (same commands
  // as .claude/launch.json). Postgres on :5433 must be up beforehand.
  webServer: [
    {
      command: `dotnet run --project ../Backend/PhysioLink.API/PhysioLink.API.csproj --urls ${API_URL}`,
      url: `${API_URL}/health`,
      reuseExistingServer: true,
      timeout: 180_000,
    },
    {
      command: `dotnet run --project ../Backend/PhysioLink.AdminPanel/PhysioLink.AdminPanel.csproj --urls ${ADMIN_URL}`,
      url: `${ADMIN_URL}/health`,
      reuseExistingServer: true,
      timeout: 180_000,
    },
  ],
});
