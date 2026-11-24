import { test as setup } from '@playwright/test';
import { ADMIN_STATE, adminCredentials, expectSignedIn, setLanguage, signIn } from './helpers';

// Signs in once and saves the cookies; the "admin" project reuses them.
setup('sign in as clinic admin', async ({ page, context, baseURL }) => {
  const { email, password } = adminCredentials();
  await setLanguage(context, baseURL!, 'en-US');
  await signIn(page, email, password);
  await expectSignedIn(page);
  await context.storageState({ path: ADMIN_STATE });
});
