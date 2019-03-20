import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { EmployeeSetupComponent } from './setup/employee-setup/employee-setup.component';
import { PracticeSetupComponent } from './setup/practice-setup/practice-setup.component';
import { ReviewSetupComponent } from './setup/review-setup/review-setup.component';
import { SetupComponent } from './setup/setup.component';
import { SurgerySetupComponent } from './setup/surgery-setup/surgery-setup.component';
import { TimerangeComponent } from './setup/timerange/timerange.component';



@NgModule({
  declarations: [
    AppComponent,
    SetupComponent,
    TimerangeComponent,
    PracticeSetupComponent,
    SurgerySetupComponent,
    EmployeeSetupComponent,
    ReviewSetupComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    HttpClientModule,
    ReactiveFormsModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
