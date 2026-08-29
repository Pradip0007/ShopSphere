#!/usr/bin/env bash
# ShopSphere smoke test — rerun before every big change.
# Usage:
#   BASE=https://localhost:7583 ./scripts/smoke.sh

set -euo pipefail

: "${BASE:?Set BASE to the API root URL}"
API="$BASE/api/v1"

EMAIL="smoke+$(date +%s)@shopsphere.dev"
PASSWORD="Sup3rSecret!12"

echo "== health =="

curl -k -sS "$BASE/alive" | grep -q "Healthy"

echo "== login =="

LOGIN="$(curl -k -sS -X POST "$API/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email":"smoketest2026@gmail.com",
    "password":"SmokeTest@12345"
  }')"

ALICE_JWT="$(echo "$LOGIN" | jq -r '.accessToken')"

test -n "$ALICE_JWT"
test "$ALICE_JWT" != "null"

echo "== list products =="

PRODUCTS="$(curl -k -sS "$API/products?pageSize=5")"

echo "$PRODUCTS" | jq -e '.items | length >= 1' > /dev/null

PROD_1="$(echo "$PRODUCTS" | jq -r '.items[0].id')"

test -n "$PROD_1"
test "$PROD_1" != "null"

echo "== cart =="

CART="$(curl -k -sS "$API/cart" \
  -H "Authorization: Bearer $ALICE_JWT")"

echo "$CART" | jq -e '.totalUnits >= 0' > /dev/null

echo "== add cart item =="

ADD_CART="$(curl -k -sS -X POST "$API/cart/items" \
  -H "Authorization: Bearer $ALICE_JWT" \
  -H "Content-Type: application/json" \
  -d "{\"productId\":\"$PROD_1\",\"quantity\":1}")"

echo "$ADD_CART" | jq -e '.totalUnits >= 1' > /dev/null

echo "== verify cart =="

curl -k -sS "$API/cart" \
  -H "Authorization: Bearer $ALICE_JWT" |
  jq -e '.totalUnits >= 1' > /dev/null

echo "== delete cart item =="

curl -k -sS -X DELETE "$API/cart/items/$PROD_1" \
  -H "Authorization: Bearer $ALICE_JWT" |
  jq -e '.totalUnits >= 0' > /dev/null

echo "== search =="

curl -k -sS "$API/products/search?q=widget" |
  jq -e '.items | length >= 0' > /dev/null

echo "OK"
