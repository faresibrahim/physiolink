import { expect, test } from '@playwright/test';

test.describe('first-visit language screen', () => {
  test('a new browser is sent to the language screen first', async ({ page }) => {
    await page.goto('/Patients');
    await expect(page).toHaveURL(/\/Language\?returnUrl=%2FPatients/);
    await expect(page.getByRole('button', { name: /English/ })).toBeVisible();
    await expect(page.getByRole('button', { name: /العربية/ })).toBeVisible();
  });

  test('choosing English lands on the sign-in page in English', async ({ page }) => {
    await page.goto('/');
    await page.getByRole('button', { name: /English/ }).click();
    await expect(page).toHaveURL(/\/Auth\/Login/);
    await expect(page.locator('html')).toHaveAttribute('dir', 'ltr');
    await expect(page.getByRole('button', { name: 'Sign in' })).toBeVisible();
  });

  test('choosing Arabic renders the sign-in page right-to-left', async ({ page }) => {
    await page.goto('/');
    await page.getByRole('button', { name: /العربية/ }).click();
    await expect(page).toHaveURL(/\/Auth\/Login/);
    await expect(page.locator('html')).toHaveAttribute('dir', 'rtl');
    await expect(page.locator('html')).toHaveAttribute('lang', 'ar');
  });

  test('the choice is remembered on the next visit', async ({ page }) => {
    await page.goto('/');
    await page.getByRole('button', { name: /English/ }).click();
    await expect(page).toHaveURL(/\/Auth\/Login/); // cookie is set once the POST completes
    await page.goto('/Patients');
    await expect(page).not.toHaveURL(/\/Language/);
  });
});
