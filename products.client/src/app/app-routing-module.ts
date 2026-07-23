import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ProductsOverviewComponent } from './products-overview/products-overview.component'
import { ProductDetailComponent } from './products-detail/products-detail.component'

const routes: Routes = [
  { path: '', component: ProductsOverviewComponent }, // Empty path = Home Page
  { path: 'products/:id', component: ProductDetailComponent } 
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
