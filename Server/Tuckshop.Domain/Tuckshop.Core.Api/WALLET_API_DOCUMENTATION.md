# Customer Wallet API Endpoints

## Overview
This implementation provides three separate endpoints for wallet management:
1. **Deposit Funds** - Add money to a customer's wallet
2. **Withdraw Funds** - Remove money from a customer's wallet (admin/refund)
3. **Pay from Wallet** - Automatically deduct during order creation

---

## 1. Deposit Funds

**Endpoint:** `POST /api/customers/commands/wallet/deposit`

**Request Body:**
```json
{
  "customerId": 1,
  "amount": 100.00
}
```

**Response:**
```json
{
  "customerId": 1,
  "customerName": "John Doe",
  "walletBalance": 150.00
}
```

**Use Case:** Customer tops up their wallet, admin adds bonus/credit

---

## 2. Withdraw Funds

**Endpoint:** `POST /api/customers/commands/wallet/withdraw`

**Request Body:**
```json
{
  "customerId": 1,
  "amount": 50.00,
  "reason": "Refund for cancelled order"
}
```

**Response:**
```json
{
  "customerId": 1,
  "customerName": "John Doe",
  "walletBalance": 100.00
}
```

**Use Case:** Manual withdrawal, refunds, corrections (typically admin only)

**Error Response (Insufficient Funds):**
```json
{
  "errors": {
    "": [
      "Insufficient wallet balance. Available: $30.00, Required: $50.00"
    ]
  }
}
```

---

## 3. Create Order with Wallet Payment

**Endpoint:** `POST /api/orders/commands/create`

**Request Body:**
```json
{
  "customerId": 1,
  "payFromWallet": true,
  "orderDetails": [
    {
      "productId": 5,
      "quantity": 2
    },
    {
      "productId": 8,
      "quantity": 1
    }
  ]
}
```

**Response:**
```json
{
  "orderId": 42,
  "customerName": "John Doe",
  "orderedOn": "2026-01-15T10:30:00Z",
  "orderDetails": [
    {
      "productId": 5,
      "quantity": 2,
      "value": 40.00,
      "vat": 5.22
    },
    {
      "productId": 8,
      "quantity": 1,
      "value": 25.00,
      "vat": 3.26
    }
  ]
}
```

**Note:** The wallet balance is automatically deducted when `payFromWallet: true`

---

## Error Handling

### Validation Errors
```json
{
  "errors": {
    "amount": [
      "Amount must be greater than zero"
    ]
  }
}
```

### Business Rule Errors
```json
{
  "errors": {
    "": [
      "Insufficient wallet balance. Available: $50.00, Required: $75.00"
    ]
  }
}
```

---

## Client-Side Implementation Examples

### JavaScript/TypeScript

```typescript
// 1. Deposit Funds
async function depositFunds(customerId: number, amount: number) {
  const response = await fetch('/api/customers/commands/wallet/deposit', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ customerId, amount })
  });
  return await response.json();
}

// 2. Withdraw Funds
async function withdrawFunds(customerId: number, amount: number, reason?: string) {
  const response = await fetch('/api/customers/commands/wallet/withdraw', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ customerId, amount, reason })
  });
  return await response.json();
}

// 3. Create Order with Wallet Payment
async function createOrder(customerId: number, items: {productId: number, quantity: number}[]) {
  const response = await fetch('/api/orders/commands/create', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      customerId,
      payFromWallet: true,
      orderDetails: items
    })
  });
  return await response.json();
}
```

### React Example

```tsx
function WalletManager({ customerId, balance }: Props) {
  const [amount, setAmount] = useState('');

  const handleDeposit = async () => {
    try {
      const result = await depositFunds(customerId, parseFloat(amount));
      alert(`Success! New balance: $${result.walletBalance}`);
    } catch (error) {
      alert('Deposit failed: ' + error.message);
    }
  };

  return (
    <div>
      <h3>Current Balance: ${balance}</h3>
      <input 
        type="number" 
        value={amount} 
        onChange={(e) => setAmount(e.target.value)} 
        placeholder="Amount"
      />
      <button onClick={handleDeposit}>Add Funds</button>
    </div>
  );
}
```

---

## Authorization Recommendations

Consider adding authorization attributes to restrict access:

```csharp
// Only authenticated users can deposit
[Authorize]
[HttpPost("wallet/deposit")]
public virtual Task<Customer> DepositFunds([FromBody] DepositFunds command) { ... }

// Only admins can withdraw
[Authorize(Roles = "Admin")]
[HttpPost("wallet/withdraw")]
public virtual Task<Customer> WithdrawFunds([FromBody] WithdrawFunds command) { ... }
```

---

## Next Steps

1. **Register Service:** ✅ Already added to `StartupExtensions.cs`
2. **Create Migration:** Run the migration to add WalletBalance column to Customers table
3. **Test Endpoints:** Use Swagger UI to test the endpoints
4. **Add Authorization:** Add appropriate `[Authorize]` attributes based on your security requirements
