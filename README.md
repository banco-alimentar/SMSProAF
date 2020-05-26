# SMSProAF
Azure Function to send SMS using NOS SMSPro gateway. Triggered via HTTP and shared secret

Used during Food Bank Campaigns to inform the people responsible for each supermarket about the total weight received

https://smspro.nos.pt/smspro/bancoalime/

A interface Web Service está disponível em:
https://smspro.nos.pt/smspro/smsprows.asmx?WSDL no formato Document-Literal style.
Para um cliente poder invocar os métodos do Web Service terá de usar o seguinte URL:
https://smspro.nos.pt/smspro/smsprows.asmx

## Settings
local.settings.json
Must include
```
    "secret": "TBD",
    "username": "TBD",
    "password": "TBD"
```
