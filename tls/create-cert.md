# https://medium.com/@rajanmaharjan/secure-your-mongodb-connections-ssl-tls-92e2addb3c89

# root ca
openssl genrsa -out rootCA.key 2048
openssl genrsa -des3 -out rootCA.key 2048 # password is mongo
openssl req -x509 -new -nodes -key rootCA.key -sha256 -days 1024 -out rootCA.pem

# per device
openssl genrsa -out mongodb.key 2048
# Whatever you see in the address field in your browser when you go to your device 
# must be what you put under common name, even if it’s an IP address. 
openssl req -new -key mongodb.key -out mongodb.csr # answer Common Name (eg, YOUR name) []: localhost

openssl x509 -req -in mongodb.csr -CA rootCA.pem -CAkey rootCA.key -CAcreateserial -out mongodb.crt -days 500 -sha256
cat mongodb.key mongodb.crt > mongodb.pem

# run mongo using docker-compose or the following command: mongod --tlsMode requireTLS --tlsCertificateKeyFile tls/mongodb.pem  