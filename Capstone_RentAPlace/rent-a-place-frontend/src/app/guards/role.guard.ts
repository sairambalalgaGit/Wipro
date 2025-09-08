import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const roleGuard = (allowedRoles: string[]): CanActivateFn => {
  return () => {
    const authService = inject(AuthService);
    const router = inject(Router);
    const role = authService.getRole();

    if (role && allowedRoles.includes(role)) {
      return true;
    } else {
      router.navigate(['/']);
      return false;
    }
  };
};
