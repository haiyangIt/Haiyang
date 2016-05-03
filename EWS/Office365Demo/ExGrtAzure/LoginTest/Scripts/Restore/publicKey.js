function getpublicKey() {
    return "-----BEGIN PUBLIC KEY-----\r\n" +
"MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCz+YkHpta6raSlMoqhYTKOgA/9\r\n" +
    "9unGkcgWxqYrcbR6LLqokSfewoGaGl9SLAerZcgTOgCvQLtom7q2T2KH5WFn4ljy\r\n" +
    "vCBIXrhnB4MIEaywg9FLTTATp1QPMlleKd0ZKjiJj4wMNbsqOeMETi5WwPLIRjY/\r\n" +
    "HBUIrWnDuGbD3U3RJwIDAQAB\r\n" +
    "-----END PUBLIC KEY-----\r\n";
}

function encryptByRsa(str) {
    var crypt = new JSEncrypt();

    crypt.setPublicKey(getpublicKey());
    return crypt.encrypt(str);
}

