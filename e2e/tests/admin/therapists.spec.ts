import { type Page, expect, test } from '@playwright/test';
import { adminCredentials } from '../helpers';

// One therapist's whole life in the panel: create → find → edit → deactivate.
// Serial: each step needs the one before. The last step deactivates the therapist.
test.describe.configure({ mode: 'serial' });

const run = Date.now().toString(36);
const therapist = {
  firstName: 'E2E',
  lastName: `Therapist ${run}`,
  specialty: 'Sports Rehabilitation',
  phone: '+970599000222',
  email: `e2e.therapist.${run}@example.test`,
};
const fullName = `${therapist.firstName} ${therapist.lastName}`;

test('the add-therapist form flags missing required fields', async ({ page }) => {
  await page.goto('/');
  await page.getByRole('navigation').getByRole('link', { name: 'Therapists', exact: true }).click();
  await page.getByRole('button', { name: 'Add therapist' }).first().click();

  const dialog = page.getByRole('dialog', { name: 'Add therapist' });
  await dialog.getByRole('button', { name: 'Add therapist' }).click();
  await expect(dialog.getByText('First name is required.')).toBeVisible();
});

test('create a therapist', async ({ page }) => {
  await page.goto('/');
  await page.getByRole('navigation').getByRole('link', { name: 'Therapists', exact: true }).click();
  await page.getByRole('button', { name: 'Add therapist' }).first().click();

  const dialog = page.getByRole('dialog', { name: 'Add therapist' });
  await dialog.getByLabel('First name').fill(therapist.firstName);
  await dialog.getByLabel('Last name').fill(therapist.lastName);
  await dialog.getByLabel('Specialty').fill(therapist.specialty);
  await dialog.getByLabel('Phone').fill(therapist.phone);
  await dialog.getByLabel('Email').fill(therapist.email);
  await dialog.getByRole('button', { name: 'Add therapist' }).click();

  await expect(page.getByRole('status')).toContainText('Therapist created successfully.');
});

test('find the therapist and open their profile', async ({ page }) => {
  await openTherapist(page);
  await expect(page.getByText(therapist.specialty)).toBeVisible();
});

test('edit the therapist from their profile', async ({ page }) => {
  await openTherapist(page);

  await page.getByRole('button', { name: 'Edit', exact: true }).click();
  const dialog = page.getByRole('dialog', { name: 'Edit Therapist' });
  await expect(dialog.getByLabel('Email')).toHaveValue(therapist.email);

  therapist.specialty = 'Neurological Rehabilitation';
  await dialog.getByLabel('Specialty').fill(therapist.specialty);
  await dialog.getByRole('button', { name: 'Save Changes' }).click();

  await expect(page.getByRole('status')).toContainText('Therapist updated successfully.');
  await expect(page.getByRole('heading', { name: fullName })).toBeVisible(); // stayed on the profile
  await expect(page.getByText(therapist.specialty)).toBeVisible();
});

test('deactivate the therapist', async ({ page }) => {
  await openTherapist(page);

  await page.getByRole('button', { name: 'Deactivate', exact: true }).click();
  const dialog = page.getByRole('dialog', { name: 'Deactivate Therapist' });
  await dialog.getByLabel(/password/i).fill(adminCredentials().password);
  await dialog.getByRole('button', { name: 'Deactivate Account' }).click();

  await expect(page.getByRole('status')).toContainText('Therapist deactivated successfully.');
});

// The therapist list has no search, so page through it until the card turns up.
async function openTherapist(page: Page) {
  await page.goto('/');
  await page.getByRole('navigation').getByRole('link', { name: 'Therapists', exact: true }).click();
  const listUrl = new URL(page.url());

  for (let n = 1; n <= 50; n++) {
    listUrl.searchParams.set('page', String(n));
    await page.goto(listUrl.toString());

    const cards = page.locator('.therapist-card');
    if ((await cards.count()) === 0) break;

    const card = cards.filter({ hasText: fullName });
    if ((await card.count()) > 0) {
      await card.getByRole('link', { name: 'View' }).click();
      await expect(page.getByRole('heading', { name: fullName })).toBeVisible();
      return;
    }
  }
  throw new Error(`Therapist "${fullName}" not found on any page of the list`);
}
