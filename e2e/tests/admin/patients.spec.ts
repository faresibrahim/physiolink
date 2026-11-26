import { expect, test } from '@playwright/test';
import { adminCredentials } from '../helpers';

// One patient's whole life in the panel: create → find → edit → deactivate.
// The steps build on each other, so they run in order and stop at the first failure.
// Each run uses a unique name; the last step deactivates the patient again.
test.describe.configure({ mode: 'serial' });

const run = Date.now().toString(36);
const patient = {
  firstName: 'E2E',
  lastName: `Patient ${run}`,
  username: `e2e.patient.${run}`,
  phone: '+970599000111',
  diagnosis: 'Knee sprain (E2E test)',
};
const fullName = `${patient.firstName} ${patient.lastName}`;

test('the new-patient form flags missing required fields', async ({ page }) => {
  await page.goto('/Patients');
  await page.getByRole('button', { name: 'New patient' }).first().click();

  const dialog = page.getByRole('dialog', { name: 'New patient' });
  await expect(dialog).toBeVisible();
  await dialog.getByRole('button', { name: 'Add Patient' }).click();

  await expect(dialog.getByText('First name is required.')).toBeVisible();
  await expect(dialog.getByLabel('First name')).toHaveAttribute('aria-invalid', 'true');
  await expect(page).toHaveURL(/\/Patients$/); // nothing was posted
});

test('create a patient and see their login credentials', async ({ page }) => {
  await page.goto('/Patients');
  await page.getByRole('button', { name: 'New patient' }).first().click();

  const dialog = page.getByRole('dialog', { name: 'New patient' });
  await dialog.getByLabel('First name').fill(patient.firstName);
  await dialog.getByLabel('Last name').fill(patient.lastName);
  await dialog.getByLabel('Username').fill(patient.username);
  await dialog.getByLabel('Phone').fill(patient.phone);
  await dialog.getByLabel('Diagnosis').fill(patient.diagnosis);
  await dialog.getByRole('button', { name: 'Add Patient' }).click();

  const created = page.getByRole('dialog', { name: 'Patient created' });
  await expect(created).toBeVisible();
  await expect(created).toContainText(patient.username);
  await expect(created).toContainText('Temporary Password');
  await created.getByRole('button', { name: 'Done' }).click();
  await expect(created).toBeHidden();
});

test('find the patient by name', async ({ page }) => {
  await page.goto('/Patients');
  await page.getByRole('textbox', { name: 'Search by name...' }).fill(patient.lastName);
  await page.getByRole('button', { name: 'Search', exact: true }).click();

  await expect(page).toHaveURL(/searchQuery=/);
  await expect(page.getByRole('link', { name: fullName })).toBeVisible();
});

test('edit the patient from their profile', async ({ page }) => {
  await openPatient(page);

  await page.getByRole('button', { name: 'Edit Patient' }).click();
  const dialog = page.getByRole('dialog', { name: 'Edit Patient' });
  await expect(dialog.getByLabel('Username')).toHaveValue(patient.username);

  const newDiagnosis = 'ACL reconstruction, week 2 (E2E test)';
  await dialog.getByLabel('Diagnosis').fill(newDiagnosis);
  await dialog.getByRole('button', { name: 'Save Changes' }).click();

  await expect(page.getByRole('status')).toContainText('Patient updated successfully.');
  await expect(page.getByRole('paragraph').filter({ hasText: newDiagnosis })).toBeVisible();
});

test('deactivation is refused with the wrong password', async ({ page }) => {
  await openPatient(page);
  await deactivate(page, 'Deactivate Patient', 'not-my-password');

  await expect(page.getByRole('alert').filter({ hasText: 'Incorrect password' })).toBeVisible();
  await expect(page.getByRole('heading', { name: fullName })).toBeVisible();
});

test('deactivate the patient', async ({ page }) => {
  await openPatient(page);
  await deactivate(page, 'Deactivate Patient', adminCredentials().password);

  const done = page.getByRole('dialog', { name: 'Patient deactivated' });
  await expect(done).toBeVisible();
  await expect(done).toContainText(fullName);
  await expect(page).toHaveURL(/\/Patients$/);
});

async function openPatient(page: import('@playwright/test').Page) {
  await page.goto(`/Patients?searchQuery=${encodeURIComponent(patient.lastName)}`);
  await page.getByRole('link', { name: fullName }).click();
  await expect(page.getByRole('heading', { name: fullName })).toBeVisible();
}

async function deactivate(page: import('@playwright/test').Page, dialogName: string, password: string) {
  await page.getByRole('button', { name: 'Deactivate Account' }).click();
  const dialog = page.getByRole('dialog', { name: dialogName });
  await dialog.getByLabel('Your account password').fill(password);
  await dialog.getByRole('button', { name: 'Deactivate Account' }).click();
}
