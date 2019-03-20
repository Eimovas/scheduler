import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subscription } from 'rxjs';
import { SetupFormService } from '../setup-form.service';

@Component({
    selector: 'scheduler-surgery-setup',
    templateUrl: './surgery-setup.component.html',
    styleUrls: ['./surgery-setup.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class SurgerySetupComponent implements OnInit, OnDestroy {  
    public surgeries: FormArray;
    private subscriptions: Subscription[];

    constructor(private setupFormService: SetupFormService, private fb: FormBuilder) { }

    ngOnInit() {
        this.subscriptions = [];
        this.surgeries = this.setupFormService.setupForm.get("surgeries") as FormArray;
        this.surgeries.controls.forEach(s => this.subscribeToWeekendChange(s as FormGroup));
    }

    ngOnDestroy() {
        this.subscriptions.forEach(sub => sub.unsubscribe());
    }

    public saveSurgery(surgery: FormGroup): void {
        surgery.disable();
    }

    public editSurgery(surgery: FormGroup): void {
        surgery.enable();
    }

    public removeSurgery(index: number): void {
        this.surgeries.removeAt(index);
    }

    public addNewSurgery(): void {
        const surgeryForm = this.fb.group({
            name: this.fb.control("", Validators.required),
            weekdayFrom: this.fb.control("08:00", Validators.required),
            weekdayTo: this.fb.control("17:00", Validators.required),
            weekendFrom: this.fb.control({ value: "10:00", disabled: true }, Validators.required),
            weekendTo: this.fb.control({ value: "15:00", disabled: true }, Validators.required),
            workingWeekends: false
        });

        this.subscribeToWeekendChange(surgeryForm);
        this.surgeries.push(surgeryForm);
    }

    private subscribeToWeekendChange(surgeryForm: FormGroup) : void {
        const subscription = surgeryForm
            .get("workingWeekends").valueChanges
            .subscribe(value => {
                if (value) {
                    surgeryForm.get("weekendFrom").enable();
                    surgeryForm.get("weekendTo").enable();
                }
                else {
                    surgeryForm.get("weekendFrom").disable();
                    surgeryForm.get("weekendTo").disable();
                }
            });
        this.subscriptions.push(subscription);
    }
}
