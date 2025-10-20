import { Component, OnInit } from '@angular/core';
import { DemoService } from './services/demo.service';

@Component({
  selector: 'app-root',
  standalone: true,
  template: `
    <div class="p-4">
      <h1>Prueba JWT Keycloak + .NET</h1>
      <button class="btn btn-primary" (click)="probar()">Llamar API protegida</button>
      <p *ngIf="respuesta">{{ respuesta }}</p>
    </div>
  `
})
export class AppComponent implements OnInit {
  respuesta?: string;

  constructor(private demoService: DemoService) {}

  ngOnInit(): void {}

  probar() {
    this.demoService.getDemo().subscribe({
      next: res => this.respuesta = res,
      error: err => this.respuesta = 'Error: ' + JSON.stringify(err)
    });
  }
}
