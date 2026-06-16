import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { TokenService } from './token.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenService = inject(TokenService);
  const token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjIiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJ0ZXN0QHRlc3QuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6InRlc3QiLCJleHAiOjE4MTMxNzgxMTcsImlzcyI6ImJvb2tmbGl4LWFwaSIsImF1ZCI6ImJvb2tmbGl4LWZyb250ZW5kIn0.kmLbckKn8V3BAP7KgEUPH3tylSwh0OU9GT-SK8yEhnQ";

  const publicUrls = ['/auth/login', '/auth/register'];
  const isPublic = publicUrls.some(url => req.url.includes(url))

  if(!token || isPublic) return next(req);

  const authReq = req.clone({
    headers: req.headers.set('Authorization', `Bearer ${token}`)
  })

  return next(authReq);
};
