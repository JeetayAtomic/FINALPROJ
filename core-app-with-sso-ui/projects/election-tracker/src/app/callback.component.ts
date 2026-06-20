import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ElectionService } from './election.service';

@Component({
  selector: 'et-callback',
  template: `
    <div class="splash">
      <div class="spinner"></div>
      <p>{{ message }}</p>
    </div>
  `,
  styles: [`
    .splash {
      min-height: 100vh;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      gap: 18px;
      color: #5f6368;
    }
    .spinner {
      width: 38px;
      height: 38px;
      border: 4px solid #d2d7e0;
      border-top-color: #174a9b;
      border-radius: 50%;
      animation: spin 0.8s linear infinite;
    }
    @keyframes spin { to { transform: rotate(360deg); } }
  `],
})
export class CallbackComponent implements OnInit {
  message = 'Validating your SSO token…';

  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private svc = inject(ElectionService);

  ngOnInit(): void {
    const token = this.route.snapshot.queryParamMap.get('token');
    if (!token) {
      this.router.navigate(['/denied']);
      return;
    }
    this.svc.validateToken(token).subscribe({
      next: () => this.router.navigateByUrl('/'),
      error: () => this.router.navigate(['/denied']),
    });
  }
}
