import { type BrowserContext, type Page, expect } from '@playwright/test';

// Where auth.setup.ts saves the signed-in admin's cookies.
export const ADMIN_STATE = 'playwright/.auth/admin.json';

// ASP.NET Core's culture cookie. Setting it skips the first-visit language screen.
export const CULTURE_COOKIE = '.AspNetCore.Culture';

export async function setLanguage(context: BrowserContext, baseURL: string, culture: 'en-US' | 'ar-PS') {
  await context.addCookies([
    {
      name: CULTURE_COOKIE,
      value: `c=${culture}|uic=${culture}`,
      url: baseURL,
    },
  ]);
}

export function adminCredentials() {
  const email = process.env.E2E_ADMIN_EMAIL;
  const password = process.env.E2E_ADMIN_PASSWORD;
  if (!email || !password) {
    throw new Error('Set E2E_ADMIN_EMAIL and E2E_ADMIN_PASSWORD in e2e/.env (see .env.example).');
  }
  return { email, password };
}

export async function signIn(page: Page, email: string, password: string) {
  await page.goto('/Auth/Login');
  await page.getByLabel('Email').fill(email);
  await page.getByLabel('Password', { exact: true }).fill(password);
  await page.getByRole('button', { name: 'Sign in' }).click();
}

export async function expectSignedIn(page: Page) {
  await expect(page).not.toHaveURL(/\/Auth\/Login/);
  await expect(page.getByRole('navigation')).toBeVisible();
}
