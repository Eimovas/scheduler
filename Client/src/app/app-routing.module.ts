import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { EmployeeSetupComponent } from './setup/employee-setup/employee-setup.component';
import { PracticeSetupComponent } from './setup/practice-setup/practice-setup.component';
import { ReviewSetupComponent } from './setup/review-setup/review-setup.component';
import { SetupComponent } from './setup/setup.component';
import { SurgerySetupComponent } from './setup/surgery-setup/surgery-setup.component';

const routes: Routes = [
  {
    path: "setup",
    component: SetupComponent,
    children: [
        { path: "", redirectTo: "practiceSetup", pathMatch: "full"},
        { path: "practiceSetup", component: PracticeSetupComponent},
        { path: "surgerySetup", component: SurgerySetupComponent},
        { path: "employeeSetup", component: EmployeeSetupComponent},
        { path: "reviewSetup", component: ReviewSetupComponent}
    ]
  },
  {
    path: "**",
    redirectTo: "/setup"
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
