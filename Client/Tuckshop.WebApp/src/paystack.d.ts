declare module '@paystack/inline-js' {
  export interface PaystackOptions {
    key: string;
    email: string;
    amount: number;
    currency?: string;
    onSuccess?: (transaction: any) => void;
    onCancel?: () => void;
    [key: string]: any;
  }

  export default class PaystackPop {
    newTransaction(options: PaystackOptions): void;
    checkout(options: PaystackOptions): void;
  }
}