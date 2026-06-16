import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Router } from '@angular/router';
import { TokenService } from '../auth/token.service';
import { tap } from 'rxjs/internal/operators/tap';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly apiUrl = environment.apiUrl + '/auth';
  private httpClient = inject(HttpClient);
  private tokenService = inject(TokenService);
  private router = inject(Router);

  isAuthenticated(): boolean {
    const payload = this.decodeToken();
    return payload != null && payload.exp > Date.now() / 1000;
  }

  private decodeToken(): any | null {
    const token = this.tokenService.getToken();
    if (!token) return null;
    try {
      return JSON.parse(atob(token.split('.')[1]));
    } catch {
      return null;
    }
  }

  getUserId(): number | null {
    const payload = this.decodeToken();
    if (!payload) return null;

    const nameId =
      payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];

    return nameId != null ? Number(nameId) : null;
  }

  login(email: string, password: string) {
    return this.httpClient.post<{ token: string }>(`${this.apiUrl}/login`, { email, password }).pipe(
      tap(response => {
        this.tokenService.setToken(response.token);
      })
    );
  }

  logout() {
    this.tokenService.removeToken();
    this.router.navigate(['/']);
  }

  register(email: string, password: string, username: string) {
    return this.httpClient.post(`${this.apiUrl}/register`, {email, password, username});
  }
}
