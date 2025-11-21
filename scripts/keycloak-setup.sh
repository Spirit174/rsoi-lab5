#!/bin/bash

echo "Setting up Keycloak..."

until curl -f http://localhost:8081/realms/master; do
    echo "Waiting for Keycloak..."
    sleep 5
done

echo "Keycloak is ready..."

ACCESS_TOKEN=$(curl -s -X POST \
  http://localhost:8081/realms/master/protocol/openid-connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "username=admin&password=admin&grant_type=password&client_id=admin-cli" | jq -r '.access_token')

if [ "$ACCESS_TOKEN" == "null" ]; then
    echo "Failed to get admin access token"
    exit 1
fi

echo "Access token obtained"

# Создаем realm
curl -X POST \
  http://localhost:8081/admin/realms \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "realm": "booking",
    "enabled": true,
    "displayName": "Booking"
  }'

# Создаем client
CLIENT_ID=$(curl -s -X POST \
  http://localhost:8081/admin/realms/booking/clients \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "clientId": "booking-client",
    "enabled": true,
    "publicClient": false,
    "secret": "THIS_is_VEry_secretic_for_EVERYONE-access_toGETyes",
    "directAccessGrantsEnabled": true,
    "serviceAccountsEnabled": true,
    "authorizationServicesEnabled": true,
    "protocol": "openid-connect",
    "attributes": {
      "access.token.lifespan": 30000
    }
  }' | jq -r '.id')

echo "Client ID: $CLIENT_ID"

# Создаем пользователя
curl -X POST \
  http://localhost:8081/admin/realms/booking/users \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "spirit",
    "enabled": true,
    "email": "spirit@test.com",
    "firstName": "Spirit",
    "lastName": "SpiritUser",
    "credentials": [
      {
        "type": "password",
        "value": "pass",
        "temporary": false
      }
    ]
  }'

echo "Keycloak setup completed successfully"