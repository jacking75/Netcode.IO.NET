#ifndef crypto_shorthash_siphash24_H
#define crypto_shorthash_siphash24_H

#include <stddef.h>
#include "sodium_export.h"

#ifdef __cplusplus
# ifdef __GNUC__
#  pragma GCC diagnostic ignored "-Wlong-long"
# endif
extern "C" {
#endif

/* -- 64-bit output -- */

#define crypto_shorthash_siphash24_BYTES 8U
size_t crypto_shorthash_siphash24_bytes(void);

#define crypto_shorthash_siphash24_KEYBYTES 16U
size_t crypto_shorthash_siphash24_keybytes(void);

int crypto_shorthash_siphash24(unsigned char *out, const unsigned char *in,
                               unsigned long long inlen, const unsigned char *k)
            __attribute__ ((nonnull));

#ifndef SODIUM_LIBRARY_MINIMAL
/* -- 128-bit output -- */

#define crypto_shorthash_siphashx24_BYTES 16U
size_t crypto_shorthash_siphashx24_bytes(void);

#define crypto_shorthash_siphashx24_KEYBYTES 16U
size_t crypto_shorthash_siphashx24_keybytes(void);

int crypto_shorthash_siphashx24(unsigned char *out, const unsigned char *in,
                                unsigned long long inlen, const unsigned char *k)
            __attribute__ ((nonnull));
#endif

#ifdef __cplusplus
}
#endif

#endif
