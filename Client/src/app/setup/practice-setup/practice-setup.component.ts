import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { FormArray, FormBuilder, FormControl, FormGroup } from '@angular/forms';
import * as moment from 'moment';
import { Subscription } from 'rxjs';
import { SetupFormService } from '../setup-form.service';
import { TimeRange } from '../setup.models';

@Component({
    selector: 'scheduler-practice-setup',
    templateUrl: './practice-setup.component.html',
    styleUrls: ['./practice-setup.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class PracticeSetupComponent implements OnInit, OnDestroy {
    public defaults: FormGroup;
    public operationTimes: FormArray;
    public parentForm: FormGroup;

    private subscriptions: Subscription[] = [];

    get currentMonth(): string {
        return this.operationTimes.controls[0].get('raw').value.format("MMMM YYYY");
    }

    constructor(private fb: FormBuilder, private setupFormService: SetupFormService) { }

    ngOnInit() {
        this.parentForm = this.setupFormService.setupForm;
        this.operationTimes = this.setupFormService.setupForm.get("operationTimes") as FormArray;

        if (this.operationTimes.length === 0) {
            const nextMonth = this.buildNextMonth();
            nextMonth
                .forEach(day => this.operationTimes.push(this.fb.group({
                    raw: day,
                    from: '',
                    to: ''
                })));
        }

        this.defaults = this.fb.group({
            weekdays: this.fb.group({ from: '', to: '' }),
            weekends: this.fb.group({ from: '', to: '' })
        });
        this.subscriptions.push(this.defaults.get("weekends").valueChanges.subscribe(value => this.handleDefaultChanges(value, "weekends")));
        this.subscriptions.push(this.defaults.get("weekdays").valueChanges.subscribe(value => this.handleDefaultChanges(value, "weekdays")));
    }

    ngOnDestroy() {
        this.subscriptions.forEach(s => s.unsubscribe());
    }

    public isWeekend(day: FormGroup): boolean {
        return day.get('raw').value.day() === 0 || day.get('raw').value.day() === 6;
    }

    public formatDay(day: FormControl): string {
        return day.get('raw').value.format("ddd DD-MM-YYYY")
    }

    private handleDefaultChanges(value: TimeRange, property: string): void {
        if (property === "weekends") {
            this.operationTimes.controls
                .filter(control => this.isWeekend(control as FormGroup))
                .forEach(control => control.patchValue(value));
        }
        else {
            this.operationTimes.controls
                .filter(control => this.isWeekend(control as FormGroup) === false)
                .forEach(control => control.patchValue(value));
        }
    }



    private buildNextMonth(): moment.Moment[] {
        const nextMonth = moment().add(1, 'month');
        let daysInMonth = nextMonth.daysInMonth();
        const arrDays = [];

        while (daysInMonth) {
            const current = nextMonth.date(daysInMonth);
            arrDays.push(current.clone());
            daysInMonth--;
        }

        return arrDays
            .sort((a: moment.Moment, b: moment.Moment) => a.date > b.date ? 1 : -1);
        // .reduce((sum, curr) => {
        //     if (curr.day() === 1) {
        //         sum.push([curr]);
        //     }
        //     else {
        //         sum[sum.length - 1].push(curr)
        //     }
        //     return sum;
        // },[[]])
        // .filter(f => f.length > 0);
    }
}

interface Defaults {
    weekends: TimeRange;
    weekdays: TimeRange;
}