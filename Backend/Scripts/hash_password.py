"""Print an ASP.NET Core Identity (v3) password hash for the given password.

The API verifies passwords with Identity's PasswordHasher<ApplicationUser>, so a
hash produced here is accepted at login. Used to seed accounts directly in SQL
(e.g. the store-review demo account) without a plaintext password in any file.

Usage:
    python hash_password.py            # prompts for the password (not echoed)

Format (all integers big-endian uint32):
    0x01 | prf (2 = HMACSHA512) | iterations | salt length | salt | 32-byte subkey
"""

import base64
import getpass
import hashlib
import os
import struct
import sys

PRF_HMACSHA512 = 2
ITERATIONS = 100_000  # .NET's default for v3 hashes; stored in the hash itself
SALT_BYTES = 16
SUBKEY_BYTES = 32


def identity_v3_hash(password: str) -> str:
    salt = os.urandom(SALT_BYTES)
    subkey = hashlib.pbkdf2_hmac("sha512", password.encode("utf-8"), salt, ITERATIONS, SUBKEY_BYTES)
    header = b"\x01" + struct.pack(">III", PRF_HMACSHA512, ITERATIONS, SALT_BYTES)
    return base64.b64encode(header + salt + subkey).decode("ascii")


if __name__ == "__main__":
    pw = getpass.getpass("Password: ")
    if len(pw) < 8:
        sys.exit("Password must be at least 8 characters.")
    if getpass.getpass("Repeat: ") != pw:
        sys.exit("Passwords do not match.")
    print(identity_v3_hash(pw))
