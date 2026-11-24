import { expect, test } from '@playwright/test';

// Every sidebar page loads for a signed-in admin, with its heading and no server error.
const pages = [
  { link: 'Patients', heading: 'Patients', url: /\/Patients/ },
  { link: 'Therapists', heading: 'Therapists', url: /\/Therapist/ },
  { link: 'Exercises', heading: 'Exercise library', url: /\/Exercises/ },
  { link: 'Appointments', heading: 'Appointments', url: /\/Appointments/ },
];

test('the dashboard loads', async ({ page }) => {
  await page.goto('/');
  await expect(page).toHaveURL(/\/($|Dashboard)/);
  await expect(page.getByRole('heading', { level: 1 })).toBeVisible();
});

for (const p of pages) {
  test(`sidebar → ${p.link}`, async ({ page }) => {
    const errors: string[] = [];
    page.on('pageerror', (e) => errors.push(e.message));

    await page.goto('/');
    await page.getByRole('navigation').getByRole('link', { name: p.link, exact: true }).click();

    await expect(page).toHaveURL(p.url);
    await expect(page.getByRole('heading', { level: 1, name: p.heading })).toBeVisible();
    expect(errors, 'uncaught JavaScript errors').toEqual([]);
  });
}

test('switching to Arabic from the sidebar flips the layout', async ({ page }) => {
  await page.goto('/Patients');
  await page.getByRole('button', { name: /Change language/ }).click();
  await expect(page.locator('html')).toHaveAttribute('dir', 'rtl');
  await expect(page).toHaveURL(/\/Patients/);

  // And back again from the Arabic sidebar.
  await page.getByRole('button', { name: /English/ }).click();
  await expect(page.locator('html')).toHaveAttribute('dir', 'ltr');
});
