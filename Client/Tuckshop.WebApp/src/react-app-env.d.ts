/// <reference types="react-scripts" />

type Nullable<T> = T | null;
const nameof = <T>(name: Extract<keyof T, string>): string => name;