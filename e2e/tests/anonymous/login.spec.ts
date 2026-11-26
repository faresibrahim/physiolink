import { expect, test } from '@playwright/test';
import { adminCredentials, expectSignedIn, setLanguage, signIn } from '../helpers';

test.beforeEach(async ({ context, baseURL }) => {
  await setLanguage(context, baseURL!, 'en-US');
});

test('signed-out visitors are redirected to sign in', async ({ page }) => {
  await page.goto('/Patients');
  await expect(page).toHaveURL(/\/Auth\/Login/);
});

test('a wrong password shows an error and stays on sign in', async ({ page }) => {
  const { email } = adminCredentials();
  await signIn(page, email, 'definitely-not-the-password');
  await expect(page).toHaveURL(/\/Auth\/Login/);
  await expect(page.getByRole('alert')).toContainText('Invalid email or password');
});

test('a patient account cannot sign in to the panel', async ({ page }) => {
  // Seeded patient (DbSeeder) — valid API credentials, wrong role.
  await signIn(page, 'john.smith@example.com', 'patient@123');
  await expect(page).toHaveURL(/\/Auth\/Login/);
  await expect(page.getByRole('alert')).toContainText('Invalid email or password');
});

test('the password toggle reveals and hides the password', async ({ page }) => {
  await page.goto('/Auth/Login');
  const password = page.getByLabel('Password', { exact: true });
  await password.fill('secret');
  await expect(password).toHaveAttribute('type', 'password');
  await page.getByRole('button', { name: 'Show password' }).click();
  await expect(password).toHaveAttribute('type', 'text');
  await page.getByRole('button', { name: 'Hide password' }).click();
  await expect(password).toHaveAttribute('type', 'password');
});

test('an admin can sign in and log out', async ({ page }) => {
  const { email, password } = adminCredentials();
  await signIn(page, email, password);
  await expectSignedIn(page);

  await page.getByRole('button', { name: 'Log out' }).click();
  await expect(page).toHaveURL(/\/Auth\/Login/);
  await page.goto('/');
  await expect(page).toHaveURL(/\/Auth\/Login/);
});
