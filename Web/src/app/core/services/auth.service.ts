import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Router } from '@angular/router';
import { TokenService } from '../auth/token.service';
import { tap } from 'rxjs';

type CurrentUser = { id: number; email: string; username: string } | null;

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiUrl = environment.apiUrl + '/auth';
  private httpClient = inject(HttpClient);
  private tokenService = inject(TokenService);
  router = inject(Router);
  private currentUser = signal<CurrentUser>(this.userFromToken());
  readonly user = this.currentUser.asReadonly();
  readonly isLoggedIn = computed(() => this.currentUser() !== null);

  login(email: string, password: string) {
    return this.httpClient
      .post<{ token: string }>(`${this.apiUrl}/login`, { email, password })
      .pipe(
        tap((res) => {
          this.tokenService.setToken(res.token);
          this.currentUser.set(this.userFromToken());
        }),
      );
  }

  register(email: string, password: string, username: string) {
    return this.httpClient.post(`${this.apiUrl}/register`, { email, password, username });
  }

  logout() {
    this.tokenService.removeToken();
    this.currentUser.set(null);
    this.router.navigate(['/']);
  }

  getUserId(): number | null {
    return this.currentUser()?.id ?? null;
  }

  private userFromToken(): CurrentUser {
    const payload = this.decodeToken();
    if (!payload || payload.exp <= Date.now() / 1000) return null;

    const idClaim = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
    const emailClaim =
      payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'];
    const nameClaim = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'];
    if (idClaim == null) return null;

    return { id: Number(idClaim), email: emailClaim ?? '', username: nameClaim ?? '' };
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
}
