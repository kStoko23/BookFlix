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
    const token = this.tokenService.getToken();
    if (!token) return false;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.exp > Date.now() / 1000;
    } catch (error) {
      return false;
    }
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
