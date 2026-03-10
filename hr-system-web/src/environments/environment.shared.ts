export interface AppEnvironment {
  apiBaseUrl: string;
}

declare global {
  interface Window {
    __HR_SYSTEM_CONFIG__?: Partial<AppEnvironment>;
  }
}

export function createEnvironment(defaults: AppEnvironment): AppEnvironment {
  return {
    ...defaults,
    ...(typeof window !== 'undefined' ? window.__HR_SYSTEM_CONFIG__ : undefined)
  };
}
