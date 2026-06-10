import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class TokenService {
  private readonly key = 'authToken';

  getToken(): string | null {
    return localStorage.getItem(this.key);
  }

  setToken(token: string): void {
    localStorage.setItem(this.key, token);
  }

  removeToken(): void {
    localStorage.removeItem(this.key);
  }
}
