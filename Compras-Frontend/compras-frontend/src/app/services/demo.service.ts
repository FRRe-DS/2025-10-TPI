import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class DemoService {
  private apiUrl = 'https://localhost:7061/api/demo/demo2'; // Ajustá el puerto de tu API

  constructor(private http: HttpClient) {}

  getDemo(): Observable<string> {
    return this.http.get(this.apiUrl, { responseType: 'text' });
  }
}
