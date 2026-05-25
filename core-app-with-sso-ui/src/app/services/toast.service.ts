import { Injectable } from '@angular/core';
import { MessageService } from 'primeng/api';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  
  constructor(private messageService: MessageService) { }

  addToast(responseCode: string, customMessage?: string) {
    const messageMap: { [key: string]: any } = {
      'SUCCESS': {
        severity: 'success',
        summary: 'Success',
        detail: customMessage || 'Operation completed successfully'
      },
      'ERROR': {
        severity: 'error',
        summary: 'Error',
        detail: customMessage || 'An error occurred'
      },
      'CREATED': {
        severity: 'success',
        summary: 'Success',
        detail: customMessage || 'Input Object created successfully'
      },
      'UPDATED': {
        severity: 'success',
        summary: 'Success',
        detail: customMessage || 'Input Object updated successfully'
      },
      'DELETED': {
        severity: 'success',
        summary: 'Success',
        detail: customMessage || 'Input Object deleted successfully'
      },
      'WARNING': {
        severity: 'warn',
        summary: 'Warning',
        detail: customMessage || 'An error occurred'
      }
    };

    const message = messageMap[responseCode] || {
      severity: 'info',
      summary: 'Info',
      detail: customMessage || responseCode
    };

    this.messageService.add(message);
  }

  success(message: string, summary: string = 'Success') {
    this.messageService.add({
      severity: 'success',
      summary: summary,
      detail: message
    });
  }

  error(message: string, summary: string = 'Error') {
    this.messageService.add({
      severity: 'error',
      summary: summary,
      detail: message
    });
  }

  info(message: string, summary: string = 'Info') {
    this.messageService.add({
      severity: 'info',
      summary: summary,
      detail: message
    });
  }

  warn(message: string, summary: string = 'Warning') {
    this.messageService.add({
      severity: 'warn',
      summary: summary,
      detail: message
    });
  }

  clear() {
    this.messageService.clear();
  }
}
