import { AfterViewInit, ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { SetupFormService } from './setup-form.service';
import { SetupStateService } from './setup-state.service';

@Component({
    selector: 'scheduler-setup',
    templateUrl: './setup.component.html',
    styleUrls: ['./setup.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class SetupComponent implements OnInit, OnDestroy, AfterViewInit {

    ngAfterViewInit(): void {
        this.setupFormService.enable();
        
    }

    constructor(private setupFormService: SetupFormService, private setupStateService: SetupStateService) { }

    ngOnInit() {
        // this.formSubscription = this.setupFormService.setupForm.valueChanges.subscribe(form => this.setupStateService.onChange(form));
    }

    ngOnDestroy() {
        // this.formSubscription.unsubscribe();
    }

    public save(): void {
        console.log(this.setupFormService.setupForm.value);
    }
}
