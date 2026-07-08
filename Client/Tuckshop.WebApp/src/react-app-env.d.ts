/// <reference types="react-scripts" />

declare namespace NodeJS {
	interface ProcessEnv {
		REACT_APP_PAYSTACK_PK?: string;
	}
}

type Nullable<T> = T | null;
const nameof = <T>(name: Extract<keyof T, string>): string => name;